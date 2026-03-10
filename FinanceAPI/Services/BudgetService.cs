using FinanceAPI.Data;
using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FinanceAPI.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly FinanceDBContex _context;
        private readonly ILogger<BudgetService> _logger;
        private readonly CurrentUser _currentUser;

        public BudgetService(FinanceDBContex context, 
                             ILogger<BudgetService> logger,
                             CurrentUser currentUser)
        {
            _context = context;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PaginatedList<Budget>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Budgets
                                    .Include(c => c.Category)
                                    .Include(u => u.User)
                                    .AsNoTracking();

            if (query == null)
                return ApiResponse<PaginatedList<Budget>>.FailureResponse("Empty list!");

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Category.Name.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            if (pageNumber <= 0 || pageSize <= 0)
                return ApiResponse<PaginatedList<Budget>>
                    .FailureResponse($"{nameof(pageNumber)} and {nameof(pageSize)} size must be greater than 0.");

            var result = await PaginatedList<Budget>.CreateAsync(query, pageNumber, pageSize);
            return ApiResponse<PaginatedList<Budget>>.SuccessResponse(result);

        }

        public async Task<ApiResponse<Budget>> GetById(int? id)
        {
            var budget = await _context.Budgets
                                       .Include(c => c.Category)
                                       .Include(u => u.User).FirstOrDefaultAsync(b => b.Id == id);
           
            if (budget == null)
                return ApiResponse<Budget>.FailureResponse("Budget not found!");

            return ApiResponse<Budget>.SuccessResponse(budget);
        }

        public async Task<ApiResponse<Budget>> Create(BudgetDTO model)
        {
            var categoryInDB = CheckIfCategoryIdExistsInDB(model.CategoryId);
            if(!categoryInDB)
            {
                _logger.LogWarning("Category {@Category} doesn't exists in database while adding for user {@UserId}}", model.CategoryId, _currentUser.LoggedInUser());
                return ApiResponse<Budget>.FailureResponse("The given Category doesn't exist in DB!");
            }
           
            var categoryInBudget = _context.Budgets.Any(b => b.CategoryId == model.CategoryId && 
                                                   b.UserId == _currentUser.LoggedInUser());
         
            if (categoryInBudget)
            {
                _logger.LogWarning("Category {@Category} already exists in budget for {@UserId} while adding", model.CategoryId, _currentUser.LoggedInUser());
                return ApiResponse<Budget>.FailureResponse("Category already exists!");
            }
                

            var create = new Budget
            {
                CategoryId = model.CategoryId,
                Limit = model.Limit,
                CurrentSpent = model.CurrentSpent == null ? 0 : model.CurrentSpent,
                Remaining = model.Limit - model.CurrentSpent,
                UserId = _currentUser.LoggedInUser()
            };

            try
            {
                await _context.Budgets.AddAsync(create);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Failed to save budget for user with id {@UserId}", create.UserId);
                return ApiResponse<Budget>.FailureResponse("Failed to save budget due to a database error.");
            }

            return ApiResponse<Budget>.SuccessResponse(create);
        }

        public async Task<ApiResponse<Budget>> Update(BudgetDTO update, int? id)
        {
            var categoryInDB = CheckIfCategoryIdExistsInDB(update.CategoryId);
            if (!categoryInDB)
            {
                _logger.LogWarning("Category {@Category} doesn't exists in database while updating for user {@UserId}", update.CategoryId, _currentUser.LoggedInUser());
                return ApiResponse<Budget>.FailureResponse("The given Category doesn't exist in DB!");
            }
                

            var existingBudget = await _context.Budgets.FindAsync(id);
            if (existingBudget == null)
            {
                _logger.LogWarning("Budget with id {@Id} not found while updating for user {@UserId}", id, _currentUser.LoggedInUser());
                return ApiResponse<Budget>.FailureResponse("Budget not found!");
            }
                

            var exists = _context.Budgets.Any(b => b.CategoryId == update.CategoryId && 
                                                   b.Id != id && b.UserId == _currentUser.LoggedInUser());
           
            if (exists)
            {
                _logger.LogWarning("Category {@Category} already exists in budget for {@UserId} while updating", update.CategoryId, _currentUser.LoggedInUser());
                return ApiResponse<Budget>.FailureResponse("Category already exists!");
            }

            existingBudget.CategoryId = update.CategoryId;
            existingBudget.Limit = update.Limit;
            existingBudget.CurrentSpent = update.CurrentSpent == null ? 0 : update.CurrentSpent;
            existingBudget.Remaining = update.Limit - update.CurrentSpent;
            existingBudget.UserId = _currentUser.LoggedInUser();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Failed to update budget for user with id {@UserId}", existingBudget.UserId);
                return ApiResponse<Budget>.FailureResponse("Failed to update budget due to a database error.");
            }

            return ApiResponse<Budget>.SuccessResponse(existingBudget);
        }

        public async Task<ApiResponse<Budget>> Delete(int? id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                _logger.LogWarning("Budget with id {@Id} not found while deleting for user {@UserId}", id, _currentUser.LoggedInUser());
                return ApiResponse<Budget>.FailureResponse("Budget not found!");
            }

            try
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete budget for user with id {@UserId}", budget.UserId);
                return ApiResponse<Budget>.FailureResponse("Failed to delete budget due to a database error.");
            }

            return ApiResponse<Budget>.SuccessResponse(budget);
        }

        private bool CheckIfCategoryIdExistsInDB(int id)
        {
            var exists = _context.Categories.Any(c => c.Id == id);
            if (exists)
                return true;

            return false;
        }
    }
}
