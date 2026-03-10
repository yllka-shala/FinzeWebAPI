using FinanceAPI.DTOs;
using FinanceAPI.Helpers;

namespace FinanceAPI.Services
{
    public interface IAdminReportsService
    {
        Task<ApiResponse<int>> CountCustomers();
        Task<ApiResponse<int>> CountUsers();
        Task<ApiResponse<double>> TotalTransactionsPerMonth();
        Task<ApiResponse<List<DailyMonthlyTransactionDTO>>> GetTransactionsLast7DaysAsync();
        Task<ApiResponse<List<DailyMonthlyTransactionDTO>>> GetTransactionsLast7MonthsAsync();
        Task<ApiResponse<int>> InActiveUsers();
    }
}
