using System.IO;
using System.Threading.Tasks;

namespace NovaCommerce.Inventory.Application
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName);
        Task DeleteFileAsync(string fileName, string containerName);
        string GetFileUrl(string fileName, string containerName);
    }
}
