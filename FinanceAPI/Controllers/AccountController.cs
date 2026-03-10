using Azure;
using FinanceAPI.DTOs;
using FinanceAPI.Models;
using FinanceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _service;

        public AccountController(IAccountService service)
        {
            _service = service;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser(RegisterUserDTO create)
        {
            var response = await _service.RegisterUser(create);

            if(response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("VerifyEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(VerificationEmailRequestDTO verificationEmail)
        {
            var response = await _service.VerifyEmail(verificationEmail.Token);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var response = await _service.Login(loginDto);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }
        
        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPaswordDTO)
        {
            var response = await _service.ForgotPassword(forgotPaswordDTO);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPaswordDTO resetPaswordDTO)
        {
            var response = await _service.ResetPassword(resetPaswordDTO);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

    }
}
