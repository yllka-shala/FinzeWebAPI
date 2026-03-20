using ClosedXML.Excel;
using FinanceAPI.Data;
using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceAPI.Services
{
    public class FinancialAccountService : IFinancialAccountService
    {
        private readonly FinanceDBContex _context;
        private readonly ILogger<FinancialAccountService> _logger;
        private readonly CurrentUser _currentUser;

        public FinancialAccountService(FinanceDBContex context,
                                       ILogger<FinancialAccountService> logger,
                                       CurrentUser currentUser)
        {
            _context = context;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PaginatedList<FinancialAccount>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.FinancialAccounts
                                                .Include(fa => fa.AccountType)
                                                .Include(u => u.User)
                                                .AsNoTracking();

            if (query == null)
                return ApiResponse<PaginatedList<FinancialAccount>>.FailureResponse("Empty list!");

            if (!_currentUser.IsAdmin())
            {
                int userId = _currentUser.LoggedInUser();
                query = query.Where(u => u.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.AccountType.Name.Contains(search) ||
                    p.Bank.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            if(pageNumber <= 0 || pageSize <= 0)
                return ApiResponse<PaginatedList<FinancialAccount>>
                    .FailureResponse($"{nameof(pageNumber)} and {nameof(pageSize)} size must be greater than 0.");

            var result = await PaginatedList<FinancialAccount>.CreateAsync(query, pageNumber, pageSize);
            return ApiResponse<PaginatedList<FinancialAccount>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<FinancialAccount>> GetById(int? id)
        {
            var financialAccount = await _context.FinancialAccounts
                                                 .Include(at => at.AccountType)
                                                 .Include(u => u.User).FirstOrDefaultAsync(f => f.Id == id);
            if (financialAccount == null)
                return ApiResponse<FinancialAccount>.FailureResponse("Financial account not found!");

            return ApiResponse<FinancialAccount>.SuccessResponse(financialAccount);
        }

        public async Task<ApiResponse<FinancialAccount>> Create(FinancialAccountDTO model)
        {
            var create = new FinancialAccount
            {
                AccountTypeId = model.AccountTypeId,
                AccountBalance = model.AccountBalance,
                Bank = model.Bank,
                UserId = _currentUser.LoggedInUser()
            };

            try
            {
                await _context.FinancialAccounts.AddAsync(create);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Failed to save financial accunt for user with id {@UserId}", _currentUser.LoggedInUser());
                return ApiResponse<FinancialAccount>.FailureResponse("Failed to save financial accunt due to a database error.");
            }

            return ApiResponse<FinancialAccount>.SuccessResponse(create);
        }

        public async Task<ApiResponse<FinancialAccount>> Update(FinancialAccountDTO update, int? id)
        {
            var existingFinancialAccounts = await _context.FinancialAccounts.FindAsync(id);
            if (existingFinancialAccounts == null)
            {
                _logger.LogWarning("Financial account with id {@Id} not found while updating for user {@UserId}", id, _currentUser.LoggedInUser());
                return ApiResponse<FinancialAccount>.FailureResponse("Financial account not found!");
            }            

            existingFinancialAccounts.AccountTypeId = update.AccountTypeId;
            existingFinancialAccounts.AccountBalance = update.AccountBalance;
            existingFinancialAccounts.Bank = update.Bank;
            existingFinancialAccounts.UserId = _currentUser.LoggedInUser();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update financial accunt for user with id {@UserId}", _currentUser.LoggedInUser());
                return ApiResponse<FinancialAccount>.FailureResponse("Failed to update financial accunt due to a database error.");
            }

            return ApiResponse<FinancialAccount>.SuccessResponse(existingFinancialAccounts);
        }

        public async Task<ApiResponse<FinancialAccount>> Delete(int? id)
        {
            var financialAccount = await _context.FinancialAccounts.FindAsync(id);
            if (financialAccount == null)
            {
                _logger.LogWarning("Financial account with id {@Id} not found while deleting for user {@UserId}", id, _currentUser.LoggedInUser());
                return ApiResponse<FinancialAccount>.FailureResponse("Financial account not found!");
            }

            try
            {
                _context.FinancialAccounts.Remove(financialAccount);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete financial accunt for user with id {@UserId}", _currentUser.LoggedInUser());
                return ApiResponse<FinancialAccount>.FailureResponse("Failed to delete financial accunt due to a database error.");
            }

            return ApiResponse<FinancialAccount>.SuccessResponse(financialAccount);
        }

        public async Task<ApiResponse<List<AccountType>>> GetAccountTypes()
        {
            var accountType = await _context.AccountTypes.ToListAsync();
            if (accountType == null)
                return ApiResponse<List<AccountType>>.FailureResponse("Empty list!");

            return ApiResponse<List<AccountType>>.SuccessResponse(accountType);
        }

        public async Task<byte[]> ExportExcel()
        {
            var financialAccounts = await _context.FinancialAccounts
                                                .Include(fa => fa.AccountType)
                                                .Include(u => u.User)
                                                .ToListAsync();

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                financialAccounts = financialAccounts.Where(t => t.UserId == userId).ToList();
            }

            var exportData = financialAccounts.Select(u => new
            {
                Id = u.Id,
                AccountType = u.AccountType!.Name,
                AccountBalance = $"{u.AccountBalance}$",
                Bank = u.Bank,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("FinancialAccount");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
