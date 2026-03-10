using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace FinanceAPI.Services
{
    public interface IAccountService
    {
        Task<ApiResponse<LoginResponseDTO>> Login(LoginDTO loginDto);
        Task<ApiResponse<UserResponseDTO>> RegisterUser(RegisterUserDTO create);
        Task<ApiResponse<VerificationEmailResponseDTO>> VerifyEmail(string token);
        Task<ApiResponse<ForgotPasswordResponseDTO>> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO);
        Task<ApiResponse<ResetPasswodResponseDTO>> ResetPassword(ResetPaswordDTO resetPasswordDto);
    }
}
