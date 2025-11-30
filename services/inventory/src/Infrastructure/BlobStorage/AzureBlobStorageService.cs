using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using NovaCommerce.Inventory.Application;
using NovaCommerce.Inventory.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace NovaCommerce.Inventory.Infrastructure.BlobStorage
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly StorageOptions _storageOptions;

        public AzureBlobStorageService(IOptions<StorageOptions> storageOptions)
        {
            _storageOptions = storageOptions.Value;
            _blobServiceClient = new BlobServiceClient(_storageOptions.ConnectionString);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, true);

            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public string GetFileUrl(string fileName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }
    }
}
