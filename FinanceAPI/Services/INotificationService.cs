using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface INotificationService
    {
        Task<ApiResponse<PaginatedList<Notification>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<List<Notification>>> GetNotifications();
        Task<ApiResponse<Notification>> GetById(int? id);
        Task<ApiResponse<Notification>> Create(NotificationDTO create);
        Task<ApiResponse<Notification>> Update(NotificationDTO update, int? id);
        Task<ApiResponse<Notification>> Delete(int? id);
        Task<ApiResponse<List<NotificationType>>> GetNotificationTypes();
        Task<ApiResponse<List<Notification>>> ClearAll();
    }
}
