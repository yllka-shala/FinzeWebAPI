namespace FinanceAPI.DTOs
{
    public class ExportRecurringDataDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
        public string DueDate { get; set; }
        public string Frequency { get; set; }

        public string CreatedDate { get; set; }
        public string User { get; set; }
    }
}
