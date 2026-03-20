using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface ICategoryService
    {
        Task<ApiResponse<PaginatedList<CategoryWithUserDto>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<List<CategoryWithUserDto>>> GetCategories();
        Task<ApiResponse<Category>> GetById(int? id);
        Task<ApiResponse<Category>> Create(CategoryDTO create);
        Task<ApiResponse<Category>> Update(CategoryDTO category, int? id);
        Task<ApiResponse<Category>> Delete(int? id);
        Task<byte[]> ExportExcel();
    }
}
