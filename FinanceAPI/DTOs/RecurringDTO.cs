using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class RecurringDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int FrequencyId { get; set; }
    }
}
