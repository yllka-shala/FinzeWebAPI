namespace FinanceAPI.DTOs
{
    public class ExportBudgetDTO
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Limit { get; set; } = string.Empty;
        public string CurrentSpent { get; set; } = string.Empty;
        public string Remaining { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
    }
}
