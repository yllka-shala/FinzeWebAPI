using FinanceAPI.Data;
using FinanceAPI.DTOs;
using FinanceAPI.Enums;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinanceAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly FinanceDBContex _context;
        private readonly ILogger<TransactionService> _logger;
        private readonly CurrentUser _currentUser;

        public TransactionService(FinanceDBContex context, 
                                  ILogger<TransactionService> logger, 
                                  CurrentUser currentUser)
        {
            _context = context;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PaginatedList<Transaction>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Transactions
                                            .Include(c => c.Category)
                                            .Include(pm => pm.PaymentMethod)
                                            .Include(u => u.User)
                                            .AsNoTracking();

            if (query == null)
                return ApiResponse<PaginatedList<Transaction>>.FailureResponse("Empty list!");

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Description.Contains(search) ||
                    p.Category.Name.Contains(search) ||
                    p.PaymentMethod.Name.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            if (pageNumber <= 0 || pageSize <= 0)
                return ApiResponse<PaginatedList<Transaction>>
                    .FailureResponse($"{nameof(pageNumber)} and {nameof(pageSize)} size must be greater than 0.");

            var result = await PaginatedList<Transaction>.CreateAsync(query, pageNumber, pageSize);
            return ApiResponse<PaginatedList<Transaction>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<Transaction>> GetById(int? id)
        {
            var transaction = await _context.Transactions
                                            .Include(c => c.Category)
                                            .Include(pm => pm.PaymentMethod)
                                            .Include(u => u.User).FirstOrDefaultAsync(u => u.Id == id);

            if (transaction == null)
                return ApiResponse<Transaction>.FailureResponse("Transaction not found!");

            return ApiResponse<Transaction>.SuccessResponse(transaction);
        }

        public async Task<ApiResponse<Transaction>> Create(TransactionDTO model)
        {
            var categoryInDB = CheckIfCategoryIdExistsInDB(model.CategoryId);
            if (!categoryInDB)
                return ApiResponse<Transaction>.FailureResponse("The given Category doesn't exist in DB!");

            var exists = _context.Transactions.Any(b => b.Description == model.Description && 
                                                        b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return ApiResponse<Transaction>.FailureResponse("Description already exists!");

            var checkBudget = await CheckBudget(model.Amount, model.CategoryId);
            if (checkBudget == false)
                return ApiResponse<Transaction>.FailureResponse("Not enouugh Budget for this category!");

            var transaction = new Transaction
            {
                Description = model.Description,
                Amount = model.Amount,
                CategoryId = model.CategoryId,
                TransactionDate = model.TransactionDate,
                PaymentMethodId = model.PaymentMethodId,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Transactions.AddAsync(transaction);

            // Update the corresponding budget
            await UpdateBudgetOnTransactionCreate(transaction.CategoryId, transaction.Amount, transaction.UserId);

            await _context.SaveChangesAsync();

            return ApiResponse<Transaction>.SuccessResponse(transaction);
        }

        public async Task<ApiResponse<Transaction>> Update(UpdateTransactionDTO update, int? id)
        {
            var existingTransaction = await _context.Transactions.FindAsync(id);
            if (existingTransaction == null)
                return ApiResponse<Transaction>.FailureResponse("Transaction not found!");

            var exists = _context.Transactions.Any(b => b.Description == update.Description && 
                                                        b.Id != id && b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return ApiResponse<Transaction>.FailureResponse("Description already exists!");

            existingTransaction.Description = update.Description;
            existingTransaction.TransactionDate = update.TransactionDate;
            existingTransaction.PaymentMethodId = update.PaymentMethodId;
            existingTransaction.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return ApiResponse<Transaction>.SuccessResponse(existingTransaction);
        }

        public async Task<ApiResponse<Transaction>> Delete(int? id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return ApiResponse<Transaction>.FailureResponse("Transaction not found!");

            _context.Transactions.Remove(transaction);

            // Update the corresponding budget
            await UpdateBudgetOnTransactionDelete(transaction.CategoryId, transaction.Amount);

            await _context.SaveChangesAsync();

            return ApiResponse<Transaction>.SuccessResponse(transaction);
        }

        public async Task<ApiResponse<List<PaymentMethod>>> GetPaymentMethods()
        {
            var paymentMethods = await _context.PaymentMethods.ToListAsync();

            if (paymentMethods == null)
                return ApiResponse<List<PaymentMethod>>.FailureResponse("Empty list!");

            return ApiResponse<List<PaymentMethod>>.SuccessResponse(paymentMethods);
        }

        private bool CheckIfCategoryIdExistsInDB(int id)
        {
            var exists = _context.Categories.Any(c => c.Id == id);
            if (exists)
                return true;

            return false;
        }

        private async Task<ApiResponse<Budget>> UpdateBudgetOnTransactionCreate(int CategoryId, double Amount, int userId)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.CategoryId == CategoryId);
            if (budget == null)
                return ApiResponse<Budget>.FailureResponse("Category doesn't match!");

            if (budget != null)
            {
                budget.CurrentSpent += Amount;

                if (budget.CurrentSpent > budget.Limit)
                {
                    _logger.LogWarning("Current spent amount has exceed the limit. Resetting it to Limit.");

                    await SendNotification(budget.CategoryId, userId);

                    budget.CurrentSpent = budget.Limit;
                }

                budget.Remaining = budget.Limit - budget.CurrentSpent;

                if (budget.Remaining < 0)
                {
                    _logger.LogWarning("Budget remaining was negative. Resetting to 0.");

                    budget.Remaining = 0;
                }

                _context.Budgets.Update(budget);
                await _context.SaveChangesAsync();
            }

            return ApiResponse<Budget>.SuccessResponse(budget);
        }

        private async Task<ApiResponse<Budget>> UpdateBudgetOnTransactionDelete(int CategoryId, double Amount)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.CategoryId == CategoryId);
            if (budget == null)
                return ApiResponse<Budget>.FailureResponse("Category doesn't match!");

            if (budget != null)
            {
                budget.CurrentSpent -= Amount;
                budget.Remaining = budget.Limit - budget.CurrentSpent;

                _context.Budgets.Update(budget);
                await _context.SaveChangesAsync();
            }

            return ApiResponse<Budget>.SuccessResponse(budget);
        }

        private async Task<ApiResponse<Notification>> SendNotification(int categoryId, int userId)
        {
            var category = await _context.Categories.Where(p => p.Id == categoryId)
                                                            .Select(p => p.Name).FirstOrDefaultAsync();

            var notification = new Notification
            {
                NotificationTypeId = (int)NotificationTypeIds.Budget_Exceeded,
                Message = category,
                Read = false,
                UserId = userId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return ApiResponse<Notification>.SuccessResponse(notification);
        }

        private async Task<bool> CheckBudget(double amount, int categoryId)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.CategoryId == categoryId);
            if (amount > budget.Remaining)
            {
                await SendNotification(categoryId, _currentUser.LoggedInUser());
                return false;
            }

            return true;
        }

    }
}
