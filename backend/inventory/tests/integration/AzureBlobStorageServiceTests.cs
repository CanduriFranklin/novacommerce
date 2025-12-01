using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using NovaCommerce.Inventory.Infrastructure.BlobStorage;
using NovaCommerce.Inventory.Infrastructure.Configuration;
using Xunit;
using Microsoft.Extensions.Options;

namespace NovaCommerce.Inventory.IntegrationTests
{
    public class AzureBlobStorageServiceTests
    {
        private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
        private readonly Mock<BlobContainerClient> _mockBlobContainerClient;
        private readonly Mock<BlobClient> _mockBlobClient;
        private readonly AzureBlobStorageService _azureBlobStorageService;

        public AzureBlobStorageServiceTests()
        {
            _mockBlobServiceClient = new Mock<BlobServiceClient>();
            _mockBlobContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();

            _mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                                  .Returns(_mockBlobContainerClient.Object);
            _mockBlobContainerClient.Setup(x => x.CreateIfNotExistsAsync(
                It.IsAny<PublicAccessType>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<Azure.Response<bool>>().Object); // Mock the response

            _mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                                    .Returns(_mockBlobClient.Object);

            _mockBlobClient.Setup(x => x.Uri).Returns(new Uri("http://mocked.blob.core.windows.net/container/file.txt"));

            var storageOptions = Options.Create(new StorageOptions { ConnectionString = "UseDevelopmentStorage=true" });
            _azureBlobStorageService = new AzureBlobStorageService(storageOptions);
        }

        [Fact]
        public async Task UploadFileAsync_ShouldReturnFileUrl()
        {
            // Arrange
            var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            var fileName = "testfile.txt";
            var containerName = "test-container";

            _mockBlobClient.Setup(x => x.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<Azure.Response<BlobContentInfo>>().Object); // Mock the response

            // Act
            var result = await _azureBlobStorageService.UploadFileAsync(fileStream, fileName, containerName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(fileName, result);
            _mockBlobContainerClient.Verify(x => x.CreateIfNotExistsAsync(
                It.IsAny<PublicAccessType>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _mockBlobClient.Verify(x => x.UploadAsync(fileStream, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldCallDeleteIfExists()
        {
            // Arrange
            var fileName = "testfile.txt";
            var containerName = "test-container";

            _mockBlobClient.Setup(x => x.DeleteIfExistsAsync(
                It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<Azure.Response<bool>>().Object); // Mock the response

            // Act
            await _azureBlobStorageService.DeleteFileAsync(fileName, containerName);

            // Assert
            _mockBlobClient.Verify(x => x.DeleteIfExistsAsync(
                DeleteSnapshotsOption.None,
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
