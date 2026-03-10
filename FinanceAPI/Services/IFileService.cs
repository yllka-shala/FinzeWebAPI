using FinanceAPI.Helpers;

namespace FinanceAPI.Services
{
    public interface IFileService
    {
        Task<string> SaveFile(IFormFile file);
        void DeleteFile(string fileName);
    }
}
