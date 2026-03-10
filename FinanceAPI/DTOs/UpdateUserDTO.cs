using FinanceAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class UpdateUserDTO
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public bool IsActive { get; set; }
    }
}
