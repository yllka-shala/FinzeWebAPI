using ClosedXML.Excel;
using FinanceAPI.Data;
using FinanceAPI.DTOs;
using FinanceAPI.Enums;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace FinanceAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly FinanceDBContex _context;
        private readonly CurrentUser _currentUser;

        public NotificationService(FinanceDBContex context,
                                   CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PaginatedList<Notification>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(u => u.User)
                                             .AsNoTracking();

            if (query == null)
                return ApiResponse<PaginatedList<Notification>>.FailureResponse("Empty list!");

            if (!_currentUser.IsAdmin())
            {
                int userId = _currentUser.LoggedInUser();
                query = query.Where(u => u.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.NotificationType.Name.Contains(search) ||
                    p.Message.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            var result = await PaginatedList<Notification>.CreateAsync(query, pageNumber, pageSize);
            return ApiResponse<PaginatedList<Notification>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<List<Notification>>> GetNotifications()
        {
            var notification = await _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(u => u.User)
                                             .Where(g => g.UserId == _currentUser.LoggedInUser()).ToListAsync();

            if (notification == null)
                return ApiResponse<List<Notification>>.FailureResponse("Empty list!");

            return ApiResponse<List<Notification>>.SuccessResponse(notification);
        }

        public async Task<ApiResponse<Notification>> GetById(int? id)
        {
            var notification = await _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(n => n.User).FirstOrDefaultAsync(nt => nt.Id == id);
            
            if (notification == null)
                return ApiResponse<Notification>.FailureResponse("Notification not found!");

            return ApiResponse<Notification>.SuccessResponse(notification);
        }

        public async Task<ApiResponse<Notification>> Create(NotificationDTO model)
        {
            var exists = _context.Notifications.Any(b => b.NotificationTypeId == model.NotificationTypeId &&
                                                         b.UserId == _currentUser.LoggedInUser());

            if (exists)
                return ApiResponse<Notification>.FailureResponse("NotificationType already exists!");

            var create = new Notification
            {
                NotificationTypeId = model.NotificationTypeId,
                Message = model.Message,
                Read = model.Read,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Notifications.AddAsync(create);
            await _context.SaveChangesAsync();

            return ApiResponse<Notification>.SuccessResponse(create);
        }

        public async Task<ApiResponse<Notification>> Update(NotificationDTO update, int? id)
        {
            var existingNotification = await _context.Notifications.FindAsync(id);
            if (existingNotification == null)
                return ApiResponse<Notification>.FailureResponse("Notification not found!");

            var exists = _context.Notifications.Any(b => b.NotificationTypeId == update.NotificationTypeId && 
                                                         b.Id != id && b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return ApiResponse<Notification>.FailureResponse("NotificationType already exists!");

            existingNotification.NotificationTypeId = update.NotificationTypeId;
            existingNotification.Message = update.Message;
            existingNotification.Read = update.Read;
            existingNotification.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return ApiResponse<Notification>.SuccessResponse(existingNotification);
        }

        public async Task<ApiResponse<Notification>> Delete(int? id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return ApiResponse<Notification>.FailureResponse("Notification not found!");

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return ApiResponse<Notification>.SuccessResponse(notification);
        }

        public async Task<ApiResponse<List<NotificationType>>> GetNotificationTypes()
        {
            var notificationTypes = await _context.NotificationTypes.ToListAsync();
            if (notificationTypes == null)
                return ApiResponse<List<NotificationType>>.FailureResponse("Notification not found!");

            return ApiResponse<List<NotificationType>>.SuccessResponse(notificationTypes);
        }

        public async Task<ApiResponse<List<Notification>>> ClearAll()
        {
            var notifications = await _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(u => u.User)
                                             .Where(t => t.UserId == _currentUser.LoggedInUser() && t.Read == false)
                                             .ToListAsync();

            if (notifications == null)
                return ApiResponse<List<Notification>>.FailureResponse("Notifications not found!");

            foreach (var n in notifications)
            {
                n.Read = true;
            }
            await _context.SaveChangesAsync();

            return ApiResponse<List<Notification>>.SuccessResponse(notifications);
        }

        public async Task<byte[]> ExportExcel()
        {
            var notifications = await _context.Notifications
                                             .Include(nt => nt.NotificationType)
                                             .Include(u => u.User)
                                             .ToListAsync();

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                notifications = notifications.Where(t => t.UserId == userId).ToList();
            }

            var exportData = notifications.Select(u => new
            {
                Id = u.Id,
                NotificationType = u.NotificationType!.Name,
                Message = u.Message,
                Read = u.Read,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Notifications");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
