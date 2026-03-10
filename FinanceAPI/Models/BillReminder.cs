using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.Models
{
    public class BillReminder
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double AmountDue { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
