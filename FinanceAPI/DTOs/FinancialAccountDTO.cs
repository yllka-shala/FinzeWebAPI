using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class FinancialAccountDTO
    {
        [Required]
        public int AccountTypeId { get; set; }

        [Required]
        public double AccountBalance { get; set; }

        public string? Bank { get; set; }
    }
}
