using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface IUserService
    {
        Task<ApiResponse<PaginatedList<UserResponseDTO>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<UserResponseDTO>> GetUserById(int? id);
        Task<ApiResponse<UserResponseDTO>> UpdateUser(UpdateUserDTO update, int? id);
        Task<ApiResponse<UserResponseDTO>> DeleteUser(int? id);
        Task<ApiResponse<UserResponseDTO>> ChangePasswordAsync(ChangePasswordDTO changePasswordDto);
        Task<ApiResponse<List<Role>>> GetAllRoles();
        Task<ApiResponse<UserResponseDTO>> UpdateProfile(UserProfileDTO profile, int? id);
        Task<ApiResponse<UserResponseDTO>> DeleteProfilePicture(int id);
        Task<byte[]> ExportExcel();
    }
}
