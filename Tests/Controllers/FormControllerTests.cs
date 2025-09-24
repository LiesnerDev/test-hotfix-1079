using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyApp.Controllers;
using MyApp.DTOs;
using MyApp.Services;
using Xunit;

namespace MyApp.Tests.Controllers
{
    public class FormControllerTests
    {
        [Fact]
        public async Task RefreshForms_ReturnsOkResult_WithForms()
        {
            // Arrange
            var forms = new List<FormDto>
            {
                new FormDto { Id = 1, Name = "Test Form", FormType = "VBForm" }
            };
            var mockService = new Mock<IFormService>();
            mockService.Setup(x => x.RefreshFormsAsync(false)).ReturnsAsync(forms);

            var controller = new FormController(mockService.Object);

            // Act
            var result = await controller.RefreshForms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(forms, okResult.Value);
        }

        [Fact]
        public async Task GetFormDetails_FormExists_ReturnsOkResult()
        {
            // Arrange
            var form = new FormDto { Id = 1, Name = "Detail Form", FormType = "VBForm" };
            var mockService = new Mock<IFormService>();
            mockService.Setup(x => x.GetFormDetailsAsync(1)).ReturnsAsync(form);

            var controller = new FormController(mockService.Object);

            // Act
            var result = await controller.GetFormDetails(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(form, okResult.Value);
        }

        [Fact]
        public async Task GetFormDetails_FormDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var mockService = new Mock<IFormService>();
            mockService.Setup(x => x.GetFormDetailsAsync(2)).ReturnsAsync((FormDto)null);
            var controller = new FormController(mockService.Object);

            // Act
            var result = await controller.GetFormDetails(2);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GenerateCode_ValidRequest_ReturnsCodeOrFilePath()
        {
            // Arrange
            var codeSample = "print('Hello World')";
            var mockService = new Mock<IFormService>();
            mockService.Setup(x => x.GeneratePythonCodeAsync(It.IsAny<GenerateCodeRequest>()))
                .ReturnsAsync(codeSample);
            var controller = new FormController(mockService.Object);
            var request = new GenerateCodeRequest { FormId = 1 };

            // Act
            var result = await controller.GenerateCode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Check if the returned anonymous object contains property "code"
            var returnedValue = okResult.Value as dynamic;
            Assert.Equal(codeSample, returnedValue.code);
        }

        [Fact]
        public async Task GenerateCode_OnException_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IFormService>();
            mockService.Setup(x => x.GeneratePythonCodeAsync(It.IsAny<GenerateCodeRequest>()))
                .ThrowsAsync(new Exception("Error generating code"));
            var controller = new FormController(mockService.Object);
            var request = new GenerateCodeRequest { FormId = 1 };

            // Act
            var result = await controller.GenerateCode(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedValue = badRequestResult.Value as dynamic;
            Assert.Equal("Error generating code", (string)returnedValue.error);
        }

        [Fact]
        public async Task PreviewCode_WhenPreviewFails_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IFormService>();
            mockService.Setup(x => x.PreviewPythonCodeAsync(It.IsAny<GenerateCodeRequest>()))
                .ReturnsAsync(false);
            var controller = new FormController(mockService.Object);
            var request = new GenerateCodeRequest { FormId = 1 };

            // Act
            var result = await controller.PreviewCode(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedValue = badRequestResult.Value as dynamic;
            Assert.Equal("Python interpreter not configured or error on preview.", (string)returnedValue.message);
        }

        [Fact]
        public async Task PreviewCode_WhenPreviewSucceeds_ReturnsOkResult()
        {
            // Arrange
            var mockService = new Mock<IFormService>();
            mockService.Setup(x => x.PreviewPythonCodeAsync(It.IsAny<GenerateCodeRequest>()))
                .ReturnsAsync(true);
            var controller = new FormController(mockService.Object);
            var request = new GenerateCodeRequest { FormId = 1 };

            // Act
            var result = await controller.PreviewCode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = okResult.Value as dynamic;
            Assert.Equal("Preview launched successfully.", (string)returnedValue.message);
        }

        [Fact]
        public void ChangeLanguage_ReturnsConfirmationMessage()
        {
            // Arrange
            var mockService = new Mock<IFormService>(); // Not used in language change
            var controller = new FormController(mockService.Object);

            // Act
            var result = controller.ChangeLanguage("en-US");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = okResult.Value as dynamic;
            Assert.Equal("Language changed to en-US", (string)returnedValue.message);
        }
    }
}
