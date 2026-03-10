using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface IBudgetService
    {
        Task<ApiResponse<PaginatedList<Budget>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<Budget>> GetById(int? id);
        Task<ApiResponse<Budget>> Create(BudgetDTO budget);
        Task<ApiResponse<Budget>> Update(BudgetDTO budget, int? id);
        Task<ApiResponse<Budget>> Delete(int? id);
    }
}
