using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class GoalDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public double TargetAmount { get; set; }

        [Required]
        public double CurrentAmount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }
}
