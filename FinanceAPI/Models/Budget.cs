using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        public double Limit { get; set; }
        public double? CurrentSpent { get; set; }
        public double? Remaining { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public User? User { get; set; }

    }
}
