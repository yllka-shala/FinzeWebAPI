using ClosedXML.Excel;
using FinanceAPI.Data;
using FinanceAPI.DTOs;
using FinanceAPI.Enums;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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

        public async Task<byte[]> ExportExcel()
        {
            var exportData = await GetBillReminderExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BillReminders");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetBillReminderExportData();

            QuestPDF.Settings.License = LicenseType.Community;

            // Create the document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Bill Reminder Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(30).Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Add Header
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(2).Padding(3).Text("Id").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Name").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("AmountDue").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("DueDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("IsPaid").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CreatedDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("User").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.Name);
                            table.Cell().Padding(3).Text(item.AmountDue);
                            table.Cell().Padding(3).Text(item.DueDate);
                            table.Cell().Padding(3).Text(item.IsPaid);
                            table.Cell().Padding(3).Text(item.CreatedDate);
                            table.Cell().Padding(3).Text(item.User);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private async Task<List<ExportBillReminderDataDTO>> GetBillReminderExportData()
        {
            var query = _context.BillReminders
                                .Include(u => u.User)
                                .AsQueryable();


            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            var billReminder = await query.ToListAsync();

            return billReminder.Select(u => new ExportBillReminderDataDTO
            {
                Id = u.Id,
                Name = u.Name,
                AmountDue = $"{u.AmountDue}$",
                DueDate = u.DueDate.ToString("dd-MM-yyyy"),
                IsPaid = u.IsPaid.ToString(),
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();
        }
    }
}
