using FinanceAPI.DTOs;
using FinanceAPI.Helpers;
using FinanceAPI.Models;

namespace FinanceAPI.Services
{
    public interface ITransactionService
    {
        Task<ApiResponse<PaginatedList<Transaction>>> GetAll(string? search, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<Transaction>> GetById(int? id);
        Task<ApiResponse<Transaction>> Create(TransactionDTO create);
        Task<ApiResponse<Transaction>> Update(UpdateTransactionDTO update, int? id);
        Task<ApiResponse<Transaction>> Delete(int? id);
        Task<ApiResponse<List<PaymentMethod>>> GetPaymentMethods();
    }
}
