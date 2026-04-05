using ClosedXML.Excel;
using FinanceAPI.Data;
using FinanceAPI.DTOs;
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
    public class CategoryService : ICategoryService
    {
        private readonly FinanceDBContex _context;
        private readonly CurrentUser _currentUser;

        public CategoryService(FinanceDBContex contex, 
                               CurrentUser currentUser)
        {
            _context = contex;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PaginatedList<CategoryWithUserDto>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {

            var query = _context.Categories
                                        .Join(
                                            _context.Users,
                                            c => c.UserId,
                                            u => u.Id,
                                            (c, u) => new CategoryWithUserDto
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Date = c.Date,
                                                UserId = c.UserId,
                                                Username = u.FirstName + " " + u.LastName
                                            }
                                        );


            if (query == null)
                return ApiResponse<PaginatedList<CategoryWithUserDto>>.FailureResponse("Empty list!");

            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Username.Contains(search));
            }

            var result = await PaginatedList<CategoryWithUserDto>.CreateAsync(query, pageNumber, pageSize);
            return ApiResponse<PaginatedList<CategoryWithUserDto>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<List<CategoryWithUserDto>>> GetCategories()
        {
            var category = await _context.Categories
                                          .Join(
                                               _context.Users,
                                               c => c.UserId,
                                               u => u.Id,
                                               (c, u) => new CategoryWithUserDto
                                               {
                                                   Id = c.Id,
                                                   Name = c.Name,
                                                   Date = c.Date,
                                                   UserId = c.UserId,
                                                   Username = u.FirstName + " " + u.LastName
                                               }
                                          )
                                          .Where(c => c.UserId == _currentUser.LoggedInUser())
                                          .ToListAsync();


            if (category == null)
                return ApiResponse<List<CategoryWithUserDto>>.FailureResponse("Empty list!");

            return ApiResponse<List<CategoryWithUserDto>>.SuccessResponse(category);
        }

        public async Task<ApiResponse<Category>> GetById(int? id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return ApiResponse<Category>.FailureResponse("Category not found!");

            return ApiResponse<Category>.SuccessResponse(category);
        }

        public async Task<ApiResponse<Category>> Create(CategoryDTO model)
        {
            var exists = _context.Categories.Any(b => b.Name == model.Name && b.UserId == _currentUser.LoggedInUser());
            if (exists)
                return ApiResponse<Category>.FailureResponse("Category not found!");

            var create = new Category
            {
                Name = model.Name,
                UserId = _currentUser.LoggedInUser()
            };

            await _context.Categories.AddAsync(create);
            await _context.SaveChangesAsync();

            return ApiResponse<Category>.SuccessResponse(create);
        }

        public async Task<ApiResponse<Category>> Update(CategoryDTO update, int? id)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
                return ApiResponse<Category>.FailureResponse("Category not found!");

            var exists = _context.Categories.Any(b => b.Name == update.Name && b.Id != id && b.UserId == _currentUser.LoggedInUser());
            if (exists)
                return ApiResponse<Category>.FailureResponse("Name already exists!");

            existingCategory.Name = update.Name;
            existingCategory.UserId = _currentUser.LoggedInUser();

            await _context.SaveChangesAsync();

            return ApiResponse<Category>.SuccessResponse(existingCategory);
        }

        public async Task<ApiResponse<Category>> Delete(int? id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return ApiResponse<Category>.FailureResponse("Category not found!");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return ApiResponse<Category>.SuccessResponse(category);
        }

        public async Task<byte[]> ExportExcel()
        {
            var exportData = await GetCategoryExportData();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Category");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPdf()
        {
            var exportData = await GetCategoryExportData();

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

                    page.Header().Text("Category Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(30).Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Add Header
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(2).Padding(3).Text("Id").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Name").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("Date").Bold();
                            header.Cell().BorderBottom(2).Padding(3).Text("User").Bold();
                        });

                        // Add Data Rows
                        foreach (var item in exportData)
                        {
                            table.Cell().Padding(3).Text(item.Id.ToString());
                            table.Cell().Padding(3).Text(item.Name);
                            table.Cell().Padding(3).Text(item.Date);
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

        private async Task<List<ExportCategoryDataDTO>> GetCategoryExportData()
        {
            var query = _context.Categories
                                       .Join(
                                           _context.Users,
                                           c => c.UserId,
                                           u => u.Id,
                                           (c, u) => new CategoryWithUserDto
                                           {
                                               Id = c.Id,
                                               Name = c.Name,
                                               Date = c.Date,
                                               UserId = c.UserId,
                                               Username = u.FirstName + " " + u.LastName
                                           }
                                       ).AsQueryable();


            if (!_currentUser.IsAdmin())
            {
                var userId = _currentUser.LoggedInUser();
                query = query.Where(t => t.UserId == userId);
            }

            var category = await query.ToListAsync();

            return category.Select(u => new ExportCategoryDataDTO
            {
                Id = u.Id,
                Name = u.Name,
                Date = u.Date.ToString("dd-MM-yyyy"),
                User = u.Username
            }).ToList();
        }

    }
}
