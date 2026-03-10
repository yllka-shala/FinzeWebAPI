using System.ComponentModel.DataAnnotations;

namespace FinanceAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int NotificationTypeId { get; set; }
        public NotificationType? NotificationType { get; set; }

        [Required]
        public string Message { get; set; }
        public bool Read { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
       
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
