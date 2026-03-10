using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class CategoryDTO
    {
        [Required]
        public string Name { get; set; }
    }
}
