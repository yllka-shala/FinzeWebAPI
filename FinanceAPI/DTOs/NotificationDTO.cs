using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.DTOs
{
    public class NotificationDTO
    {
        [Required]
        public int NotificationTypeId { get; set; }

        [Required]
        public string Message { get; set; }
        public bool Read { get; set; }
    }
}
