namespace FinanceAPI.DTOs
{
    public class ExportNotificationDataDTO
    {
        public int Id { get; set; }
        public string NotificationType { get; set; }
        public string Message { get; set; }
        public string Read { get; set; }
        public string CreatedDate { get; set; }
        public string User { get; set; }
    }
}
