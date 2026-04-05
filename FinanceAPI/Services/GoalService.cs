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
using System.Collections.Generic;
using System.Security.Claims;

namespace FinanceAPI.Services
{
    public class GoalService : IGoalService
    {
        private readonly FinanceDBContex _context;
        private readonly CurrentUser _currentUser;

        public GoalService(FinanceDBContex contex,
                           CurrentUser currentUser)
        {
            _context = contex;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PaginatedList<Goal>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Goals
                                    .Include(u => u.User)
                                    .AsNoTracking();

            if (query == null)
                return ApiResponse<PaginatedList<Goal>>.FailureResponse("Empty list!");

            if (!_currentUser.IsAdmin())
            {
                int userId = _currentUser.LoggedInUser();
                query = query.Where(u => u.UserId == userId);
            }


            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search));
            }

            if (pageNumber <= 0 || pageSize <= 0)
                return ApiResponse<PaginatedList<Goal>>
                    .FailureResponse($"{nameof(pageNumber)} and {nameof(pageSize)} size must be greater than 0.");

            var result = await PaginatedList<Goal>.CreateAsync(query, pageNumber, pageSize);
            return ApiResponse<PaginatedList<Goal>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<Goal>> GetById(int? id)
        {
            var goal = await _context.Goals
                                     .Include(u => u.User).FirstOrDefaultAsync(g => g.Id == id);
            
            if (goal == null)
                return ApiResponse<Goal>.FailureResponse("Goal not found!");

            return ApiResponse<Goal>.SuccessResponse(goal);
        }

        public async Task<ApiResponse<Goal>> Create(GoalDTO model)
        {
            var exists = _context.Goals.Any(b => b.Name == model.Name && 
                                                 b.UserId == _currentUser.LoggedInUser());
            
            if (exists)
                return ApiResponse<Goal>.FailureResponse("Name already exists!");

            var create = new Goal
            {
                Name = model.Name,
                TargetAmount = model.TargetAmount,
                CurrentAmount = model.CurrentAmount,
                DueDate = model.DueDate,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Goals.AddAsync(create);
            await _context.SaveChangesAsync();

            return ApiResponse<Goal>.SuccessResponse(create);
        }

        public async Task<ApiResponse<Goal>> Update(GoalDTO update, int? id)
        {
            var existingGoal = await _context.Goals.FindAsync(id);
            if (existingGoal == null)
                return ApiResponse<Goal>.FailureResponse("Goal not found!");

            var exists = _context.Goals.Any(b => b.Name == update.Name && 
                                                 b.Id != id && 
                                                 b.UserId == _currentUser.LoggedInUser());
           
            if (exists)
                return ApiResponse<Goal>.FailureResponse("Name already exists!");

            existingGoal.Name = update.Name;
            existingGoal.TargetAmount = update.TargetAmount;
            existingGoal.CurrentAmount = update.CurrentAmount;
            existingGoal.DueDate = update.DueDate;
            existingGoal.UserId = _currentUser.LoggedInUser();


            if (existingGoal.CurrentAmount == existingGoal.TargetAmount)
            {
                //Send notification
                await SendNotification(existingGoal.UserId);
            }

            await _context.SaveChangesAsync();

            return ApiResponse<Goal>.SuccessResponse(existingGoal);
        }

        public async Task<ApiResponse<Goal>> Delete(int? id)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
                return ApiResponse<Goal>.FailureResponse("Goal not found!");

            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();

            return ApiResponse<Goal>.SuccessResponse(goal);
        }

        private async Task<ApiResponse<Notification>> SendNotification(int userId)
        {

            var notification = new Notification
            {
                NotificationTypeId = (int)NotificationTypeIds.Goal_Reached,
                Message = nameof(NotificationTypeIds.Goal_Reached),
                Read = false,
                UserId = userId
            };

            await _context.Notifications.AddAsync(notification);

            return ApiResponse<Notification>.SuccessResponse(notification);
        }

        public async Task<byte[]> ExportExcel()
        {
            var exportData = await GetGoalExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Goals");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Column(3).Style.NumberFormat.Format = "#,##0.00$";
            worksheet.Column(4).Style.NumberFormat.Format = "#,##0.00$";

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetGoalExportData();

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

                    page.Header().Text("Goal Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

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
                            columns.RelativeColumn();
                        });

                        // Add Header
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(2).Padding(3).Text("Id").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Name").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("TargetAmount").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CurrentAmount").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Progress").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("DueDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("CreatedDate").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("User").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.Name);
                            table.Cell().Padding(3).Text(item.TargetAmount.ToString() + "$");
                            table.Cell().Padding(3).Text(item.CurrentAmount.ToString() + "$");
                            table.Cell().Padding(3).Text(item.Progress.ToString("F2"));
                            table.Cell().Padding(3).Text(item.DueDate);
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

        private async Task<List<ExportGoalDataDTO>> GetGoalExportData()
        {
            var query = _context.Goals
                                .Include(u => u.User)
                                .AsQueryable();


            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            var goal = await query.ToListAsync();

            return goal.Select(u => new ExportGoalDataDTO
            {
                Id = u.Id,
                Name = u.Name,
                TargetAmount = u.TargetAmount,
                CurrentAmount = u.CurrentAmount,
                Progress = u.CurrentAmount / u.TargetAmount * 100,
                DueDate = u.DueDate.ToString("dd-MM-yyyy"),
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy"),
                User = $"{u.User!.FirstName} {u.User.LastName}"
            }).ToList();
        }
    }
}
