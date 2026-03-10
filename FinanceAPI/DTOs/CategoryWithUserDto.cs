namespace FinanceAPI.DTOs
{
    public class CategoryWithUserDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
