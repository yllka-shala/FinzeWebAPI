using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class UpdateTransactionDTO
    {
        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }
    }
}
