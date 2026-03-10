using FinanceAPI.DTOs;
using FinanceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] string? search, int pageNumber = 1, int pageSize = 10)
        {
            var response = await _service.GetAll(search, pageNumber, pageSize);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var response = await _service.GetNotifications();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetById(id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(NotificationDTO create)
        {
            var response = await _service.Create(create);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPut("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(NotificationDTO update, int id)
        {
            var response = await _service.Update(update, id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.Delete(id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("GetNotificationTypes")]
        public async Task<IActionResult> GetNotificationTypes()
        {
            var response = await _service.GetNotificationTypes();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("ClearAll")]
        public async Task<IActionResult> ClearAll()
        {
            var response = await _service.ClearAll();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }
    }
}
