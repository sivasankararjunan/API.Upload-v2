using API.Upload_v2.Utilities;
using API.Upload_v2.Validator;
using Castle.Core.Configuration;
using FileUploadService;
using FileUploadService.Models;
using FileUploadService.Process;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

using _FileUploadService = FileUploadService.Process.FileUploadService;
namespace API.Upload_v2.Test
{
    public class FileUploadService_Test
    {
        Mock<ILogger<_FileUploadService>> logger = new Mock<ILogger<_FileUploadService>>();
        Mock<IHttpClientFactory> httpClientFactory = new Mock<IHttpClientFactory>();
        Mock<Microsoft.Extensions.Configuration.IConfiguration> configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        Mock<ISchemaValidator> schemaValidator = new Mock<ISchemaValidator>();
        Mock<HttpClient> httpClient = new Mock<HttpClient>();

        _FileUploadService fileUploadService;


        public FileUploadService_Test()
        {
            var metaInfo = new MetaInfo();
            var vendorInformation = new List<VendorInformation>() {
            new VendorInformation{ AppID="1",containerName="Container",Filenamestructure="FileNameStructure", Schemaname="SchemaName" }
            };
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient.Object);
            fileUploadService = new _FileUploadService(httpClientFactory.Object,
                metaInfo, vendorInformation, schemaValidator.Object, logger.Object, configuration.Object);

        }

        [Theory]
        [InlineData("1", "year:2023", "")]
        [InlineData("1", "", "")]
        public async Task ProcessDataAsync_Success_FileNameStructure(string appId, string metaData, string data)
        {
            var vendorInformation = new List<VendorInformation>() {
            new VendorInformation{ AppID="1",containerName="Container",Filenamestructure="FileNameStructure_{year}", Schemaname="SchemaName" }
            };
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient.Object);
            var _fileUploadService = new _FileUploadService(httpClientFactory.Object,
                new MetaInfo(), vendorInformation, schemaValidator.Object, logger.Object, configuration.Object);

            httpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(async () => new HttpResponseMessage(HttpStatusCode.Created));
            configuration.Setup(x => x["SchemaType"]).Returns("NJSON");
            if (!string.IsNullOrEmpty(metaData))
            {
                var response = await _fileUploadService.UploadFile(appId, data, metaData, new CancellationToken());
                Assert.IsType<HttpResponseMessage>(response);
                Assert.Equal(response.StatusCode, HttpStatusCode.Created);
            }
            else
            {
                Assert.ThrowsAsync<BadHttpRequestException>(() => _fileUploadService.UploadFile(appId, data, metaData, new CancellationToken()));
            }
        }


        [Theory]
        [InlineData("1", "", "")]
        public async Task ProcessDataAsync_Success_NJ(string appId, string metaData, string data)
        {
            httpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(async () => new HttpResponseMessage(HttpStatusCode.Created));
            configuration.Setup(x => x["SchemaType"]).Returns("NJSON");
            var response = await fileUploadService.UploadFile(appId, metaData, data, new CancellationToken());
            Assert.IsType<HttpResponseMessage>(response);
            Assert.Equal(response.StatusCode, HttpStatusCode.Created);
        }

        [Theory]
        [InlineData("1", "", "")]
        public async Task ProcessDataAsync_Success_NSJ(string appId, string metaData, string data)
        {
            httpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(async () => new HttpResponseMessage(HttpStatusCode.Created));
            configuration.Setup(x => x["SchemaType"]).Returns("NJSONs");
            var response = await fileUploadService.UploadFile(appId, metaData, data, new CancellationToken());
            Assert.IsType<HttpResponseMessage>(response);
            Assert.Equal(response.StatusCode, HttpStatusCode.Created);
        }

        [Theory]
        [InlineData("2", "", "")]
        public async Task ProcessDataAsync_Error_WrongAPp(string appId, string metaData, string data)
        {
            httpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(async () => new HttpResponseMessage(HttpStatusCode.Created));

            var response = fileUploadService.UploadFile(appId, metaData, data, new CancellationToken());

            await Assert.ThrowsAsync<BadHttpRequestException>(() => response);
        }

        [Theory]
        [InlineData("1")]
        public async Task ProcessDataAsync_Error_StorageAccountError(string appId)
        {

            httpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(async () => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var response = fileUploadService.UploadFile(appId, "", "", new CancellationToken());
            await Assert.ThrowsAnyAsync<BadHttpRequestException>(() => response);
        }

        [Theory]
        [InlineData("1")]
        public async Task ProcessDataAsync_Error_StorageAccountBadRequest(string appId)
        {
            httpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(async () => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var response = fileUploadService.UploadFile(appId, "", "", new CancellationToken());
            await Assert.ThrowsAnyAsync<Exception>(() => response);
        }
    }
}