using FinanceAPI.Data;
using FinanceAPI.DTOs;
using FinanceAPI.Enums;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceAPI.Services
{
    public class BillReminderService : IBillReminderService
    {

        private readonly FinanceDBContex _context;
        private readonly CurrentUser _currentUser;

        public BillReminderService(FinanceDBContex context, 
                                   CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PaginatedList<BillReminder>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.BillReminders
                                            .Include(u => u.User)
                                            .AsNoTracking();

            if (query == null)
                return ApiResponse<PaginatedList<BillReminder>>.FailureResponse("Empty list!");


            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            if (pageNumber <= 0 || pageSize <= 0)
                return ApiResponse<PaginatedList<BillReminder>>
                    .FailureResponse($"{nameof(pageNumber)} and {nameof(pageSize)} size must be greater than 0.");

            var result = await PaginatedList<BillReminder>.CreateAsync(query, pageNumber, pageSize);
            return ApiResponse<PaginatedList<BillReminder>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<BillReminder>> GetById(int? id)
        {
            var billReminder = await _context.BillReminders
                                             .Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
           
            if (billReminder == null)
                return ApiResponse<BillReminder>.FailureResponse("Bill reminder not found!");

            return ApiResponse<BillReminder>.SuccessResponse(billReminder);
        }

        public async Task<ApiResponse<BillReminder>> Create(BillReminderDTO model)
        {
            var exists = _context.BillReminders.Any(b => b.Name == model.Name && 
                                                         b.UserId == _currentUser.LoggedInUser());
           
            if (exists)
                return ApiResponse<BillReminder>.FailureResponse("Name already exist!");

            var create = new BillReminder
            {
                Name = model.Name,
                AmountDue = model.AmountDue,
                DueDate = model.DueDate,
                IsPaid = model.IsPaid,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.BillReminders.AddAsync(create);

            //Send notification
            await SendNotification(create.Name, create.UserId);

            await _context.SaveChangesAsync();

            return ApiResponse<BillReminder>.SuccessResponse(create);
        }

        public async Task<ApiResponse<BillReminder>> Update(BillReminderDTO update, int? id)
        {
            var existingBillReminder = await _context.BillReminders.FindAsync(id);
            if (existingBillReminder == null)
                return ApiResponse<BillReminder>.FailureResponse("Bill reminder not found!");

            var exists = _context.BillReminders.Any(b => b.Name == update.Name && 
                                                         b.Id != id && b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return ApiResponse<BillReminder>.FailureResponse("Name already exist!");

            existingBillReminder.Name = update.Name;
            existingBillReminder.AmountDue = update.AmountDue;
            existingBillReminder.DueDate = update.DueDate;
            existingBillReminder.IsPaid = update.IsPaid;
            existingBillReminder.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return ApiResponse<BillReminder>.SuccessResponse(existingBillReminder);
        }

        public async Task<ApiResponse<BillReminder>> Delete(int? id)
        {
            var billReminder = await _context.BillReminders.FindAsync(id);
            if (billReminder == null)
                return ApiResponse<BillReminder>.FailureResponse("Bill reminder not found!");

            _context.BillReminders.Remove(billReminder);
            await _context.SaveChangesAsync();

            return ApiResponse<BillReminder>.SuccessResponse(billReminder);
        }

        private async Task<ApiResponse<Notification>> SendNotification(string name, int userId)
        {
            var notification = new Notification
            {
                NotificationTypeId = (int)NotificationTypeIds.Bill_Reminder,
                Message = name,
                Read = false,
                UserId = userId
            };

            await _context.Notifications.AddAsync(notification);
            return ApiResponse<Notification>.SuccessResponse(notification);
        }
    }
}
