using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.Models
{
    public class Goal
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double TargetAmount { get; set; }

        [Required]
        public double CurrentAmount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User? User { get; set; }
        public double Progress => CurrentAmount / TargetAmount * 100;
    }
}
