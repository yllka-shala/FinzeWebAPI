using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class TransactionDTO
    {
        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount needs to be higher than 0")]
        public double Amount { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }
    }
}
