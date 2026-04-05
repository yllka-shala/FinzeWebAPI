namespace FinanceAPI.DTOs
{
    public class ExportUserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string CreatedDate { get; set; }
        public string IsActive { get; set; }
    }
}
