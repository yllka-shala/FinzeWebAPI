using Azure;
using FinanceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IAdminReportsService _service;

        public AdminReportsController(IAdminReportsService service)
        {
            _service = service;
        }

        [HttpGet("CountCustomers")]
        public async Task<IActionResult> CountCustomers()
        {
            var response = await _service.CountCustomers();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("CountUsers")]
        public async Task<IActionResult> CountUsers()
        {
            var response = await _service.CountUsers();

            if(response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("TotalTransactionsPerMonth")]
        public async Task<IActionResult> TotalTransactionsPerMonth()
        {
            var response = await _service.TotalTransactionsPerMonth();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("GetTransactionsLast7DaysAsync")]
        public async Task<IActionResult> GetTransactionsLast7DaysAsync()
        {
            var response = await _service.GetTransactionsLast7DaysAsync();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("GetTransactionsLast7MonthsAsync")]
        public async Task<IActionResult> GetTransactionsLast7MonthsAsync()
        {
            var response = await _service.GetTransactionsLast7MonthsAsync();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("InActiveUsers")]
        public async Task<IActionResult> InActiveUsers()
        {
            var response = await _service.InActiveUsers();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }
    }
}
