namespace FinanceAPI.DTOs
{
    public class ExportFinancialAccountDataDTO
    {
        public int Id { get; set; }

        public string AccountType { get; set; }
        public string AccountBalance { get; set; }

        public string Bank { get; set; }
        public string CreatedDate { get; set; }

        public string User { get; set; }
    }
}
