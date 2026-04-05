namespace FinanceAPI.DTOs
{
    public class ExportBillReminderDataDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string AmountDue { get; set; }

        public string DueDate { get; set; }

        public string IsPaid { get; set; }
        public string CreatedDate { get; set; }
        public string User { get; set; }
    }
}
