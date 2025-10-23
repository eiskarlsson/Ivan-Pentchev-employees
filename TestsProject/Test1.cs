using Ivan_Pentchev_employees.Server.Controllers;
using Ivan_Pentchev_employees.Server.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace Ivan_Pentchev_employees.Server.Tests.Controllers
{
    [TestClass]
    public class FileUploadControllerTests
    {
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private Mock<ILogger<FileUploadController>> _mockLogger;
        private Mock<IEmployeeAlgorithm> _mockAlgorithm;
        private FileUploadController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<FileUploadController>>();
            _mockAlgorithm = new Mock<IEmployeeAlgorithm>();
            _controller = new FileUploadController(_mockEnvironment.Object, _mockLogger.Object, _mockAlgorithm.Object);
        }

        [TestMethod]
        public async Task UploadSingle_NoFile_ReturnsBadRequest()
        {
            // Arrange
            IFormFile file = null;

            // Act
            var result = await _controller.UploadSingle(file);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            var uploadResult = badRequestResult.Value as FileUploadResult;
            Assert.IsFalse(uploadResult.Success);
            Assert.AreEqual("No file uploaded", uploadResult.Message);
        }

        [TestMethod]
        public async Task UploadSingle_EmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            mockFile.Setup(f => f.FileName).Returns("test.csv");

            // Act
            var result = await _controller.UploadSingle(mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            var uploadResult = badRequestResult.Value as FileUploadResult;
            Assert.IsFalse(uploadResult.Success);
            Assert.AreEqual("No file uploaded", uploadResult.Message);
        }

        [TestMethod]
        public async Task UploadSingle_FileTooLarge_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(11 * 1024 * 1024); // 11MB
            mockFile.Setup(f => f.FileName).Returns("test.csv");

            // Act
            var result = await _controller.UploadSingle(mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            var uploadResult = badRequestResult.Value as FileUploadResult;
            Assert.IsFalse(uploadResult.Success);
            Assert.AreEqual("File size exceeds 10MB limit", uploadResult.Message);
        }

        [TestMethod]
        public async Task UploadSingle_InvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.txt");

            // Act
            var result = await _controller.UploadSingle(mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
            var badRequestResult = result.Result as BadRequestObjectResult;
            var uploadResult = badRequestResult.Value as FileUploadResult;
            Assert.IsFalse(uploadResult.Success);
            Assert.AreEqual("Invalid file type", uploadResult.Message);
        }

        [TestMethod]
        public async Task UploadSingle_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.csv");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UploadSingle(mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = result.Result as ObjectResult;
            Assert.AreEqual(500, objectResult.StatusCode);

            var uploadResult = objectResult.Value as FileUploadResult;
            Assert.IsFalse(uploadResult.Success);
            Assert.AreEqual("Internal server error", uploadResult.Message);
        }

        [TestMethod]
        public void ToJson_SerializesObject_ReturnsValidJson()
        {
            // Arrange
            var testObject = new { Name = "Test", Value = 123 };

            // Act
            var result = _controller.ToJson(testObject);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Test"));
            Assert.IsTrue(result.Contains("123"));
        }

    }
}
