using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class BudgetDTO
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public double Limit { get; set; }

        public double? CurrentSpent { get; set; }

        public double? Remaining { get; set; }
    }
}
