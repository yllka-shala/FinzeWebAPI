using ClosedXML.Excel;
using FinanceAPI.Data;
using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace FinanceAPI.Services
{
    public class RecurringService : IRecurringService
    {
        private readonly FinanceDBContex _context;
        private readonly CurrentUser _currentUser;

        public RecurringService(FinanceDBContex context,
                                CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PaginatedList<Recurring>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Recurrings
                                        .Include(f => f.Frequency)
                                        .Include(u => u.User)
                                        .AsNoTracking();

            if (query == null)
                return ApiResponse<PaginatedList<Recurring>>.FailureResponse("Empty list!");

            if (!_currentUser.IsAdmin())
            {
                int userId = _currentUser.LoggedInUser();
                query = query.Where(u => u.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Frequency.Name.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            if (pageNumber <= 0 || pageSize <= 0)
                return ApiResponse<PaginatedList<Recurring>>
                    .FailureResponse($"{nameof(pageNumber)} and {nameof(pageSize)} size must be greater than 0.");

            var result = await PaginatedList<Recurring>.CreateAsync(query, pageNumber, pageSize);
            return ApiResponse<PaginatedList<Recurring>>.SuccessResponse(result);
        }
        public async Task<ApiResponse<Recurring>> GetById(int? id)
        {
            var recurring = await _context.Recurrings
                                          .Include(f => f.Frequency)
                                          .Include(u => u.User).FirstOrDefaultAsync(f => f.Id == id);
            
            if (recurring == null)
                return ApiResponse<Recurring>.FailureResponse("Recurring not found!");

            return ApiResponse<Recurring>.SuccessResponse(recurring);
        }

        public async Task<ApiResponse<Recurring>> Create(RecurringDTO model)
        {
            var exists = _context.Recurrings.Any(b => b.Name == model.Name && 
                                                      b.UserId == _currentUser.LoggedInUser());
           
            if (exists)
                return ApiResponse<Recurring>.FailureResponse("Name already exists!");

            var create = new Recurring
            {
                Name = model.Name,
                Amount = model.Amount,
                DueDate = model.DueDate,
                FrequencyId = model.FrequencyId,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Recurrings.AddAsync(create);
            await _context.SaveChangesAsync();

            return ApiResponse<Recurring>.SuccessResponse(create);
        }

        public async Task<ApiResponse<Recurring>> Update(RecurringDTO update, int? id)
        {
            var existingRecuring = await _context.Recurrings.FindAsync(id);
            if (existingRecuring == null)
                return ApiResponse<Recurring>.FailureResponse("Recurring not found!");

            var exists = _context.Recurrings.Any(b => b.Name == update.Name && 
                                                      b.Id != id && 
                                                      b.UserId == _currentUser.LoggedInUser());
           
            if (exists)
                return ApiResponse<Recurring>.FailureResponse("Name already exists!");

            existingRecuring.Name = update.Name;
            existingRecuring.Amount = update.Amount;
            existingRecuring.DueDate = update.DueDate;
            existingRecuring.FrequencyId = update.FrequencyId;
            existingRecuring.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return ApiResponse<Recurring>.SuccessResponse(existingRecuring);
        }

        public async Task<ApiResponse<Recurring>> Delete(int? id)
        {
            var notification = await _context.Recurrings.FindAsync(id);
            if (notification == null)
                return ApiResponse<Recurring>.FailureResponse("Recurring not found!");

            _context.Recurrings.Remove(notification);
            await _context.SaveChangesAsync();

            return ApiResponse<Recurring>.SuccessResponse(notification);
        }

        public async Task<ApiResponse<List<Frequency>>> GetFrequencies()
        {
            var frequencies = await _context.Frequencies.ToListAsync();
            if (frequencies == null)
                return ApiResponse<List<Frequency>>.FailureResponse("Empty list!");

            return ApiResponse<List<Frequency>>.SuccessResponse(frequencies);
        }

        public async Task<byte[]> ExportExcel()
        {
            var recurrings = await _context.Recurrings
                                        .Include(f => f.Frequency)
                                        .Include(u => u.User)
                                        .ToListAsync();

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                recurrings = recurrings.Where(t => t.UserId == userId).ToList();
            }

            var exportData = recurrings.Select(u => new
            {
                Id = u.Id,
                Name = u.Name,
                Amount = $"{u.Amount}$",
                DueDate = u.DueDate.ToString("dd-MM-yyyy"),
                Frequency = u.Frequency!.Name,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Recurrings");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
