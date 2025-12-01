using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NovaCommerce.Inventory.Application;
using Xunit;
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace NovaCommerce.Inventory.UnitTests
{
    public class ImageServiceTests
    {
        private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
        private readonly ImageService _imageService;

        public ImageServiceTests()
        {
            _blobStorageServiceMock = new Mock<IBlobStorageService>();
            _imageService = new ImageService(_blobStorageServiceMock.Object);
        }

        [Fact]
        public async Task UploadImageAsync_ShouldReturnImageUrl_WhenImageIsValid()
        {
            // Arrange
            var fileName = "test.jpg";
            var imageUrl = "http://test.blob.core.windows.net/product-images/test.jpg";
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("dummy content")));

            _blobStorageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                                   .ReturnsAsync(imageUrl);

            // Act
            var result = await _imageService.UploadImageAsync(mockFile.Object);

            // Assert
            Assert.Equal(imageUrl, result);
            _blobStorageServiceMock.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "product-images"), Times.Once);
        }

        [Fact]
        public async Task DeleteImageAsync_ShouldCallBlobStorageService_WhenImageUrlIsValid()
        {
            // Arrange
            var imageUrl = "http://test.blob.core.windows.net/product-images/some-guid.jpg";
            _blobStorageServiceMock.Setup(s => s.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                                   .Returns(Task.CompletedTask);

            // Act
            await _imageService.DeleteImageAsync(imageUrl);

            // Assert
            _blobStorageServiceMock.Verify(s => s.DeleteFileAsync("some-guid.jpg", "product-images"), Times.Once);
        }

        [Fact]
        public async Task DeleteImageAsync_ShouldDoNothing_WhenImageUrlIsNullOrEmpty()
        {
            // Act
            await _imageService.DeleteImageAsync(null);
            await _imageService.DeleteImageAsync(string.Empty);

            // Assert
            _blobStorageServiceMock.Verify(s => s.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
