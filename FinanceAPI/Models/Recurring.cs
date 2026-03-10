using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.Models
{
    public class Recurring
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int FrequencyId { get; set; }
        public Frequency? Frequency { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
      
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
