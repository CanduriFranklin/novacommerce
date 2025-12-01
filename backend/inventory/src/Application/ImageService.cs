using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NovaCommerce.Inventory.Application
{
    public class ImageService
    {
        private readonly IBlobStorageService _blobStorageService;
        private const string ContainerName = "product-images";

        public ImageService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<string> UploadImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            await using var stream = image.OpenReadStream();
            return await _blobStorageService.UploadFileAsync(stream, fileName, ContainerName);
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            // Extract filename from URL
            var uri = new Uri(imageUrl);
            var fileName = Path.GetFileName(uri.LocalPath);

            await _blobStorageService.DeleteFileAsync(fileName, ContainerName);
        }
    }
}
