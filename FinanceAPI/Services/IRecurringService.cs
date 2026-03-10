using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface IRecurringService
    {
        Task<ApiResponse<PaginatedList<Recurring>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<Recurring>> GetById(int? id);
        Task<ApiResponse<Recurring>> Create(RecurringDTO create);
        Task<ApiResponse<Recurring>> Update(RecurringDTO update, int? id);
        Task<ApiResponse<Recurring>> Delete(int? id);
        Task<ApiResponse<List<Frequency>>> GetFrequencies();
    }
}
