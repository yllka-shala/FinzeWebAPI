using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class BillReminderDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public double AmountDue { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; }
    }
}
