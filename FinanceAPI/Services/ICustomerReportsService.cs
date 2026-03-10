using FinanceAPI.DTOs;
using FinanceAPI.Helpers;

namespace FinanceAPI.Services
{
    public interface ICustomerReportsService
    {
        Task<ApiResponse<double>> TotalTransactionsThisMonth();
        Task<ApiResponse<List<DailyMonthlyTransactionDTO>>> GetTransactionsLast7Days();
        Task<ApiResponse<List<DailyMonthlyTransactionDTO>>> GetTransactionsLast7Months();
        Task<ApiResponse<List<CategoryChartDTO>>> GetTransactionsByCategoryChart();
        Task<ApiResponse<List<MonthlySpentChartDTO>>> GetMonthlySpentData();
        Task<ApiResponse<List<CategoryChartDTO>>> GetBudgetsByCategoryData();
        Task<ApiResponse<int>> CountFinancialAccounts();
        Task<ApiResponse<int>> CountCategories();
        Task<ApiResponse<double>> TotalBudgetPerYear();
    }
}
