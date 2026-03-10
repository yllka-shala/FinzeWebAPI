using FinanceAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
        public string? ProfilePicture { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
