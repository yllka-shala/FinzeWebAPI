using ClosedXML.Excel;
using FinanceAPI.Controllers;
using FinanceAPI.Data;
using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FinanceAPI.Services
{
    public class UserService : IUserService
    {
        private readonly FinanceDBContex _context;
        private readonly IFileService _fileService;
        private readonly ILogger<UserService> _logger;

        public UserService(FinanceDBContex context, 
                           IFileService fileService, 
                           ILogger<UserService> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedList<UserResponseDTO>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Users
                                .Include(u => u.Role)
                                .Select(u => new UserResponseDTO
                                {
                                    Id = u.Id,
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    Email = u.Email,
                                    ProfilePicture = u.ProfilePicture,
                                    RoleId = u.RoleId,
                                    Role = u.Role,
                                    CreatedDate = u.CreatedDate
                                })
                                .AsNoTracking();

            if (query == null)
                return ApiResponse<PaginatedList<UserResponseDTO>>.FailureResponse("Empty list!");


            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(search) ||
                    p.LastName.Contains(search) ||
                    p.Email.Contains($"{search}") ||
                    p.Role.Name.Contains(search));
            }

            if (pageNumber <= 0 || pageSize <= 0)
                return ApiResponse<PaginatedList<UserResponseDTO>>
                    .FailureResponse($"{nameof(pageNumber)} and {nameof(pageSize)} size must be greater than 0.");

            var result = await PaginatedList<UserResponseDTO>.CreateAsync(query, pageNumber, pageSize);
            return ApiResponse<PaginatedList<UserResponseDTO>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<UserResponseDTO>> GetUserById(int? id)
        {
            var user = await _context.Users.Include(r => r.Role).FirstOrDefaultAsync(u => u.Id == id);
            
            if (user == null)
                return ApiResponse<UserResponseDTO>.FailureResponse("User not found!");

            var userResponse = new UserResponseDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                RoleId = user.RoleId,
                Role = user.Role,
                CreatedDate = user.CreatedDate
            };

            return ApiResponse<UserResponseDTO>.SuccessResponse(userResponse);
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateUser(UpdateUserDTO update, int? id)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                _logger.LogWarning("User with id {@UserId} not found for update", id);
                return ApiResponse<UserResponseDTO>.FailureResponse("User not found!");
            }
                
            var exist = await _context.Users.AnyAsync(c => c.Email.ToLower() == update.Email.ToLower() && c.Id != update.Id);
            if (exist)
            {
                _logger.LogWarning("Email {@Email} already exists for another user during update of id {@UserId}", update.Email, id);
                return ApiResponse<UserResponseDTO>.FailureResponse("Email already exists!");
            }               

            existingUser.FirstName = update.FirstName;
            existingUser.LastName = update.LastName;
            existingUser.Email = update.Email;
            existingUser.RoleId = update.RoleId;
            existingUser.IsActive = update.IsActive;

            var userResponse = new UserResponseDTO
            {
                Id = existingUser.Id,
                FirstName = existingUser.FirstName,
                LastName = existingUser.LastName,
                Email = existingUser.Email,
                ProfilePicture = existingUser.ProfilePicture,
                RoleId = existingUser.RoleId,
                Role = existingUser.Role,
                CreatedDate = existingUser.CreatedDate
            };

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Failed to update user with id {@UserId}", id);
                return ApiResponse<UserResponseDTO>.FailureResponse("Failed to update user due to a database error.");
            }
            
            return ApiResponse<UserResponseDTO>.SuccessResponse(userResponse);
        }

        public async Task<ApiResponse<UserResponseDTO>> DeleteUser(int? id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with id {@UserId} not found for deletion", id);
                return ApiResponse<UserResponseDTO>.FailureResponse("User not found!");
            }

            var userResponse = new UserResponseDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                RoleId = user.RoleId,
                Role = user.Role,
                CreatedDate = user.CreatedDate
            };

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user with id {@UserId}", id);
                return ApiResponse<UserResponseDTO>.FailureResponse("Failed to delete user due to a database error.");
            }

            return ApiResponse<UserResponseDTO>.SuccessResponse(userResponse);
        }

        public async Task<ApiResponse<UserResponseDTO>> ChangePasswordAsync(ChangePasswordDTO changePasswordDto)
        {
            var user = await _context.Users.FindAsync(changePasswordDto.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with id {@UserId} not found for password change", changePasswordDto.UserId);
                return ApiResponse<UserResponseDTO>.FailureResponse("User not found!");
            }

            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password);
            if (!isCurrentPasswordValid)
            {
                _logger.LogWarning("Cann't verify the password for user with id {@UserId} ", changePasswordDto.UserId);
                return ApiResponse<UserResponseDTO>.FailureResponse("Invalid password!");
            }

            var userResponse = new UserResponseDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                RoleId = user.RoleId,
                Role = user.Role,
                CreatedDate = user.CreatedDate
            };

            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change password for user with id {@UserId}", changePasswordDto.UserId);
                return ApiResponse<UserResponseDTO>.FailureResponse("Failed to delete user due to a database error.");
            }
           
            return ApiResponse<UserResponseDTO>.SuccessResponse(userResponse);
        }

        public async Task<ApiResponse<List<Role>>> GetAllRoles()
        {
            var roles = await _context.Roles.ToListAsync();

            if (roles == null)
                return ApiResponse<List<Role>>.FailureResponse("Empty list!");

            return ApiResponse<List<Role>>.SuccessResponse(roles);
        }

        public async Task<ApiResponse<UserResponseDTO>> UpdateProfile(UserProfileDTO profile, int? id)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                _logger.LogWarning("User with id {@UserId} not found for profile update", id);
                return ApiResponse<UserResponseDTO>.FailureResponse("User not found!");
            }
                
            var exist = await _context.Users.AnyAsync(c => c.Email.ToLower() == profile.Email.ToLower() && c.Id != profile.Id);
            if (exist)
            {
                _logger.LogWarning("Email {@Email} already exists for another user during profile update of id {@UserId}", profile.Email, id);
                return ApiResponse<UserResponseDTO>.FailureResponse("Email already exists!");
            }                

            if (profile.ImageFile != null)
            {
                profile.ProfilePicture = await _fileService.SaveFile(profile.ImageFile);

                //Delete the old picture from folder
                if (existingUser.ProfilePicture != null)
                {
                    _fileService.DeleteFile(existingUser.ProfilePicture);
                    _logger.LogInformation("User with id {@UserId} has updated its profile picture. The old picture has been deleted: ", id);
                }

                existingUser.ProfilePicture = profile.ProfilePicture;
            }

            existingUser.FirstName = profile.FirstName;
            existingUser.LastName = profile.LastName;
            existingUser.Email = profile.Email;

            var userResponse = new UserResponseDTO
            {
                Id = existingUser.Id,
                FirstName = existingUser.FirstName,
                LastName = existingUser.LastName,
                Email = existingUser.Email,
                ProfilePicture = existingUser.ProfilePicture,
                RoleId = existingUser.RoleId,
                Role = existingUser.Role,
                CreatedDate = existingUser.CreatedDate
            };

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User with id {@UserId} failed to update profile ", id);
                return ApiResponse<UserResponseDTO>.FailureResponse("Failed to update user profile due to a database error.");
            }

            return ApiResponse<UserResponseDTO>.SuccessResponse(userResponse);
        }

        public async Task<ApiResponse<UserResponseDTO>> DeleteProfilePicture(int id)
        {
            if(id == 0)
                return ApiResponse<UserResponseDTO>.FailureResponse("Invalid Id!");

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                _logger.LogWarning("User with id {@UserId} not found for profile picture deletion", id);
                return ApiResponse<UserResponseDTO>.FailureResponse("User not found!");
            }   

            if (string.IsNullOrEmpty(existingUser.ProfilePicture))
            {
                _logger.LogWarning("Profile picture is already null for user with id {@UserId}", id);
                return ApiResponse<UserResponseDTO>.FailureResponse("Profile picture is already null.");
            }

            var userResponse = new UserResponseDTO
            {
                Id = existingUser.Id,
                FirstName = existingUser.FirstName,
                LastName = existingUser.LastName,
                Email = existingUser.Email,
                ProfilePicture = existingUser.ProfilePicture,
                RoleId = existingUser.RoleId,
                Role = existingUser.Role,
                CreatedDate = existingUser.CreatedDate
            };

            try
            {
                _fileService.DeleteFile(existingUser.ProfilePicture);
                existingUser.ProfilePicture = null;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User with id {@UserId} failed to update profile ", id);
                return ApiResponse<UserResponseDTO>.FailureResponse("Failed to update user profile due to a database error.");
            }

            return ApiResponse<UserResponseDTO>.SuccessResponse(userResponse);
        }

        public async Task<byte[]> ExportExcel()
        {
            var users = await _context.Users
                                    .Include(u => u.Role)
                                    .ToListAsync();

            var exportData = users.Select(u => new
            {
                Id = u.Id,
                User = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                Role = u.Role!.Name,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate.ToString("dd-MM-yyyy")
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            worksheet.Cell("A1").InsertTable(exportData);

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
