using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface IGoalService
    {
        Task<ApiResponse<PaginatedList<Goal>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<Goal>> GetById(int? id);
        Task<ApiResponse<Goal>> Create(GoalDTO create);
        Task<ApiResponse<Goal>> Update(GoalDTO update, int? id);
        Task<ApiResponse<Goal>> Delete(int? id);
        Task<byte[]> ExportExcel();
    }
}
