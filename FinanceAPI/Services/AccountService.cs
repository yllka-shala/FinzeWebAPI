using Azure;
using FinanceAPI.Data;
using FinanceAPI.DTOs;
using FinanceAPI.Enums;
using FinanceAPI.Helpers;
using FinanceAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FinanceAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly FinanceDBContex _context;
        private readonly ITokenService _token;
        private readonly ILogger<AccountService> _logger;
        private readonly IEmailService _emailService;

        public AccountService(FinanceDBContex context, 
                              ITokenService token, 
                              ILogger<AccountService> logger,
                              IEmailService emailService)
        {
            _context = context;
            _token = token;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<ApiResponse<LoginResponseDTO>> Login(LoginDTO loginDto)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(c => c.Email == loginDto.Email);
            if (user == null)
                return ApiResponse<LoginResponseDTO>.FailureResponse("User not found!");

            if (!user.IsActive || !user.IsVerified)
                return ApiResponse<LoginResponseDTO>.FailureResponse("InActive User or Not Verified!");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
            if (!isPasswordValid)
                return ApiResponse<LoginResponseDTO>.FailureResponse("Invalid email or password!");

            var token = _token.GenerateToken(user);

            var response = new LoginResponseDTO
            {
                Message = "Login successful.",
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                Token = token,
            };

            _logger.LogInformation("User with username {@Email} loggedIn", loginDto.Email);

            return ApiResponse<LoginResponseDTO>.SuccessResponse(response);
        }

        public async Task<ApiResponse<UserResponseDTO>> RegisterUser(RegisterUserDTO create)
        {
            var exist = await _context.Users.AnyAsync(c => c.Email.ToLower() == create.Email.ToLower());
            if (exist)
                return ApiResponse<UserResponseDTO>.FailureResponse("Email already exists!");

            var roleId = await GetRoleId();
            if(roleId == 0)
            {
                _logger.LogCritical("No role was found!");
                return ApiResponse<UserResponseDTO>.FailureResponse("No role was found!");
            }

            var user = new User
            {
                FirstName = create.FirstName,
                LastName = create.LastName,
                Email = create.Email,
                RoleId = roleId,
                IsActive = true,
                Password = BCrypt.Net.BCrypt.HashPassword(create.Password),
                VerificationToken = GenerateVerificationToken(),
                EmailTokenExpires = DateTime.Now.AddHours(24)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await SendVerificationEmail(user);

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

            _logger.LogInformation("New user registered {@Firstname} {@Lastname}!", user.FirstName, user.LastName);

            return ApiResponse<UserResponseDTO>.SuccessResponse(userResponse);
        }

        public async Task<ApiResponse<VerificationEmailResponseDTO>> VerifyEmail(string token)
        {
            var user = _context.Users.SingleOrDefault(x => x.VerificationToken == token);

            if (user == null)
                return ApiResponse<VerificationEmailResponseDTO>.FailureResponse("Verification failed");

            if (user.EmailTokenExpires < DateTime.Now)
                return ApiResponse<VerificationEmailResponseDTO>.FailureResponse("Token is no longer valid!");

            user.Verified = DateTime.Now;
            user.VerificationToken = null;

            await _context.SaveChangesAsync();

            var response = new VerificationEmailResponseDTO
            {
                Message = "Email verified successfuly, you can log in now!"
            };

            return ApiResponse<VerificationEmailResponseDTO>.SuccessResponse(response);
        }

        public async Task<ApiResponse<ForgotPasswordResponseDTO>> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            var user = _context.Users.SingleOrDefault(x => x.Email == forgotPasswordDTO.Email);

            if (user == null)
                return ApiResponse<ForgotPasswordResponseDTO>.FailureResponse("Something went wrong!");

            user.ResetToken = GenerateResetToken();
            user.ResetTokenExpires = DateTime.Now.AddHours(3);

            await _context.SaveChangesAsync();

            await SendPasswordResetEmail(user);

            var response = new ForgotPasswordResponseDTO
            {
                Email = user.Email,
                Message = "Please check your email for password reset instruction."
            };

            return ApiResponse<ForgotPasswordResponseDTO>.SuccessResponse(response);
        }

        public async Task<ApiResponse<ResetPasswodResponseDTO>> ResetPassword(ResetPaswordDTO resetPasswordDto)
        {
            var user = GetAccountByResetToken(resetPasswordDto.Token);
            if (user == null)
                return ApiResponse<ResetPasswodResponseDTO>.FailureResponse("Invalid Token!");

            if (user.IsActive == false)
                return ApiResponse<ResetPasswodResponseDTO>.FailureResponse("InActive User!");

            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            user.PasswordReset = DateTime.Now;
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            var userResponse = new ResetPasswodResponseDTO
            {
                Message = "Pasword reset successful, you can now login!"
            };

            return ApiResponse<ResetPasswodResponseDTO>.SuccessResponse(userResponse);
        }

        private async Task<int> GetRoleId()
        {
            bool adminExists = await _context.Users.AnyAsync(r => r.Role.Name == RoleType.Admin.ToString());

            string roleName = adminExists ? RoleType.Customer.ToString() : RoleType.Admin.ToString();

            int roleId = await _context.Roles.Where(r => r.Name == roleName).Select(r => r.Id).FirstOrDefaultAsync();
                
            return roleId;
        }

        private User GetAccountByResetToken(string token)
        {
            var user = _context.Users.SingleOrDefault(x => x.ResetToken == token && x.ResetTokenExpires > DateTime.Now);
            return user;
        }

        private string GenerateVerificationToken()
        {
            var token = _token.GenerateRandomToken();

            var tokenIsUnique = !_context.Users.Any(x => x.VerificationToken == token);
            if (!tokenIsUnique)
                return GenerateVerificationToken();

            return token;
        }

        private string GenerateResetToken()
        {
            var token = _token.GenerateRandomToken();

            var tokenIsUnique = !_context.Users.Any(x => x.ResetToken == token);
            if (!tokenIsUnique)
                return GenerateResetToken();

            return token;
        }

        private async Task SendVerificationEmail(User user)
        {
            string message = $@"<p>Please use the below token to verify your email address with the <code>/api/Account/VerifyEmail</code> api route:</p>
                            <p><code>{user.VerificationToken}</code></p>";


            await _emailService.SendEmailAsync(
                    to: user.Email,
                    subject: "Sign-up Verification API - Verify Email",
                    html: $@"<h4>Verify Email</h4>
                            <p>Thanks for registering!</p>
                            {message}"
            );
        }

        private async Task SendPasswordResetEmail(User user)
        {
            string message = $@"<p>Please use the below token to reset your password with the <code>/Account/ResetPassword</code> api route:</p>
                            <p><code>{user.ResetToken}</code></p>";
            

            await _emailService.SendEmailAsync(
                to: user.Email,
                subject: "Sign-up Verification API - Reset Password",
                html: $@"<h4>Reset Password Email</h4>
                        {message}"
            );
        }
    }
}
