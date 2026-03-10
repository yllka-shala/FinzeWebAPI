using FinanceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerReportsController : ControllerBase
    {
        private readonly ICustomerReportsService _service;

        public CustomerReportsController(ICustomerReportsService service)
        {
            _service = service;
        }

        [HttpGet("TotalTransactionsThisMonth")]
        public async Task<IActionResult> TotalTransactionsThisMonth()
        {
            var response = await _service.TotalTransactionsThisMonth();

            if(response.Success == false)
                return NotFound();

            return Ok(response);
        }

        [HttpGet("GetBudgetsByCategoryData")]
        public async Task<IActionResult> GetBudgetsByCategoryData()
        {
            var response = await _service.GetBudgetsByCategoryData();

            if (response.Success == false)
                return NotFound();

            return Ok(response);
        }

        [HttpGet("GetTransactionsByCategoryChart")]
        public async Task<IActionResult> GetTransactionsByCategoryChart()
        {
            var response = await _service.GetTransactionsByCategoryChart();

            if (response.Success == false)
                return NotFound();

            return Ok(response);
        }

        [HttpGet("GetMonthlySpentData")]
        public async Task<IActionResult> GetMonthlySpentData()
        {
            var response = await _service.GetMonthlySpentData();

            if (response.Success == false)
                return NotFound();

            return Ok(response);
        }

        [HttpGet("GetTransactionsLast7Days")]
        public async Task<IActionResult> GetTransactionsLast7DaysAsync()
        {
            var response = await _service.GetTransactionsLast7Days();

            if (response.Success == false)
                return NotFound();

            return Ok(response);
        }

        [HttpGet("GetTransactionsLast7Months")]
        public async Task<IActionResult> GetTransactionsLast7MonthsAsync()
        {
            var response = await _service.GetTransactionsLast7Months();

            if (response.Success == false)
                return NotFound();

            return Ok(response);
        }

        [HttpGet("CountCategories")]
        public async Task<IActionResult> CountCategories()
        {
            var response = await _service.CountCategories();

            if (response.Success == false)
                return NotFound();

            return Ok(response);
        }

        [HttpGet("CountFinancialAccounts")]
        public async Task<IActionResult> CountFinancialAccounts()
        {
            var response = await _service.CountFinancialAccounts();

            if (response.Success == false)
                return NotFound();

            return Ok(response);
        }

        [HttpGet("TotalBudgetPerYear")]
        public async Task<IActionResult> TotalBudgetPerYear()
        {
            var response = await _service.TotalBudgetPerYear();

            if (response.Success == false)
                return NotFound();

            return Ok(response);
        }
    }
}
