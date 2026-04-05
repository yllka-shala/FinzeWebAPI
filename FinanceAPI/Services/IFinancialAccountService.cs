using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface IFinancialAccountService
    {
        Task<ApiResponse<PaginatedList<FinancialAccount>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<FinancialAccount>> GetById(int? id);
        Task<ApiResponse<FinancialAccount>> Create(FinancialAccountDTO create);
        Task<ApiResponse<FinancialAccount>> Update(FinancialAccountDTO update, int? id);
        Task<ApiResponse<FinancialAccount>> Delete(int? id);
        Task<ApiResponse<List<AccountType>>> GetAccountTypes();
        Task<byte[]> ExportExcel();
        Task<byte[]> ExportPdf();
    }
}
