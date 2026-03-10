using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class ResetPaswordDTO
    {
        [Required(ErrorMessage = "Token is required!")]
        public string Token { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [MinLength(8, ErrorMessage = "New Password must be at least 8 characters.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm New Password is required.")]
        [Compare("NewPassword", ErrorMessage = "New Password and Confirm New Password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
