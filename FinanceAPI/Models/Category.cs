using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
