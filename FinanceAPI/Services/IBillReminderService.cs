using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface IBillReminderService
    {
        Task<ApiResponse<PaginatedList<BillReminder>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<BillReminder>> GetById(int? id);
        Task<ApiResponse<BillReminder>> Create(BillReminderDTO create);
        Task<ApiResponse<BillReminder>> Update(BillReminderDTO update, int? id);
        Task<ApiResponse<BillReminder>> Delete(int? id);
        Task<byte[]> ExportExcel();
        Task<byte[]> ExportPdf();
    }
}
