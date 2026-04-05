using FinanceAPI.DTOs;
using FinanceAPI.Services;
using FinanceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FinancialAccountController : ControllerBase
    {
        private readonly IFinancialAccountService _service;

        public FinancialAccountController(IFinancialAccountService service)
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
        public async Task<IActionResult> Create(FinancialAccountDTO create)
        {
            var response = await _service.Create(create);

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(FinancialAccountDTO update, int id)
        {
            var response = await _service.Update(update, id);

            if (response.Success == false)
                return NotFound(response);

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


        [HttpGet("GetAccountTypes")]
        public async Task<IActionResult> GetAccountTypes()
        {
            var response = await _service.GetAccountTypes();

            if (response.Success == false)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("ExportData")]
        public async Task<IActionResult> ExportData(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return BadRequest("Invalid format! Choose Excel or Pdf format!");
            }

            if (format.ToLower() == "excel")
            {
                var content = await _service.ExportExcel();
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"FinancialAccountReport_{DateTime.Now:dd-MM-yyyy}.xlsx";

                return File(content, contentType, fileName);
            }
            else
            {
                var content = await _service.ExportPdf();
                var contentType = "application/pdf";
                var fileName = $"FinancialAccountReport_{DateTime.Now:dd-MM-yyyy}.pdf";

                return File(content, contentType, fileName);
            }
        }
    }
}
