using FinanceAPI.DTOs;
using FinanceAPI.Services;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _service;

        public TransactionController(ITransactionService service)
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

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetById(id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(TransactionDTO create)
        {
            var response = await _service.Create(create);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateTransactionDTO update)
        {
            var response = await _service.Update(update, id);

            if (response.Success == false)
                return NotFound();

            return Ok(response);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.Delete(id);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("GetPaymentMethods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var response = await _service.GetPaymentMethods();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }
    }
}
