namespace FinanceAPI.DTOs
{
    public class LoginResponseDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }

    }
}
