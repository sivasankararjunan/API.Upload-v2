using API.Upload_v2.Utilities;
using Castle.Core.Configuration;
using FileUploadService.Controllers;
using FileUploadService.Process;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace API.Upload_v2.Test
{
    public class FileUploadController_Test
    {
        Mock<ILogger<FileUploadController>> logger = new Mock<ILogger<FileUploadController>>();
        Mock<Microsoft.Extensions.Configuration.IConfiguration> configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        Mock<IFileUploadService> fileUploadService = new Mock<IFileUploadService>();
        Mock<IHttpUtilities> httpUtilities = new Mock<IHttpUtilities>();
        FileUploadController fileUploadController;
        public FileUploadController_Test()
        {

            fileUploadController = new FileUploadController(logger.Object, configuration.Object,
                fileUploadService.Object, httpUtilities.Object);
        }

        [Theory]
        [InlineData("", "")]

        public async Task ProcessFileAsync_InvalidFileContent(string appId, string metaData)
        {
            httpUtilities.Setup(x => x.ReadFile(It.IsAny<HttpRequest>())).Throws(new BadHttpRequestException("No file content."));
            var response = await fileUploadController.ProcessFileAsync(appId, metaData);
            Assert.IsType<BadRequestObjectResult>(response);

        }
        [Theory]
        [InlineData("", "")]
        public async Task ProcessFileAsync_500Error(string appId, string metaData)
        {
            httpUtilities.Setup(x => x.ReadFile(It.IsAny<HttpRequest>())).Returns("100");
            fileUploadService.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new Exception(""));
            var response = await fileUploadController.ProcessFileAsync(appId, metaData);
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, (int)HttpStatusCode.InternalServerError);
        }
        [Theory]
        [InlineData("", "")]
        public async Task ProcessFileAsync_Success(string appId, string metaData)
        {
            httpUtilities.Setup(x => x.ReadFile(It.IsAny<HttpRequest>())).Returns("100");
            fileUploadService.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(async () => new HttpResponseMessage());
            var response = await fileUploadController.ProcessFileAsync(appId, metaData);
            Assert.IsType<OkObjectResult>(response);
        }
        [Theory]
        [InlineData("", "", "")]

        public async Task ProcessDataAsync_InvalidFileContent(string appId, string metaData, string data)
        {
            fileUploadService.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new BadHttpRequestException(""));
            var response = await fileUploadController.processDataAsync(appId, metaData, data);
            Assert.IsType<BadRequestObjectResult>(response);
        }
        [Theory]
        [InlineData("", "", "{Test}")]
        public async Task ProcessDataAsync_400InvalidContent(string appId, string metaData, string data)
        {
            fileUploadService.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new Exception(""));
            var response = await fileUploadController.processDataAsync(appId, metaData, data);
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, (int)HttpStatusCode.InternalServerError);
        }

        [Theory]
        [InlineData("", "", "{Test:'test'}")]
        public async Task ProcessDataAsync_500Error(string appId, string metaData, string data)
        {
            fileUploadService.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new Exception(""));
            var response = await fileUploadController.processDataAsync(appId, metaData, data);
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, (int)HttpStatusCode.InternalServerError);
        }

        [Theory]
        [InlineData("", "", "{Test:'test'}")]
        public async Task ProcessDataAsync_Success(string appId, string metaData, string data)
        {
            fileUploadService.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(async () => new HttpResponseMessage());
            var response = await fileUploadController.processDataAsync(appId, metaData, data);
            Assert.IsType<OkObjectResult>(response);
        }
        [Theory]
        [InlineData("", "", "")]
        public async Task ProcessDataAsync_EmptyBody(string appId, string metaData, string data)
        {
            fileUploadService.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(async () => new HttpResponseMessage());
            var response = await fileUploadController.processDataAsync(appId, metaData, data);
            Assert.IsType<BadRequestObjectResult>(response);
        }
    }
}