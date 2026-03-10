using Azure;
using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Services;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? search, int pageNumber = 1, int pageSize = 10)
        {
            var response = await _service.GetAll(search, pageNumber, pageSize);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetUserById(id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPut("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(UpdateUserDTO update, int id)
        {
            var response = await _service.UpdateUser(update, id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteUser(id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDto)
        {
            var response = await _service.ChangePasswordAsync(changePasswordDto);

            if (response.Success == false)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("GetRoles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoles()
        {
            var response = await _service.GetAllRoles();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPut("UpdateProfile/{id}")]
        public async Task<IActionResult> UpdateProfile([FromForm] UserProfileDTO profile, int id)
        {
            if (profile.ImageFile != null)
            {
                if (profile.ImageFile.Length > 1 * 1024 * 1024)
                {
                    return BadRequest("File size cannot exceed 1mb!");
                }
            }

            var response = await _service.UpdateProfile(profile, id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("RemoveProfilePicture/{id}")]
        public async Task<IActionResult> RemoveProfilePicture(int id)
        {
            var response = await _service.DeleteProfilePicture(id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }
    }
}
