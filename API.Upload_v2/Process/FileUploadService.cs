using API.Upload_v2.Validator;
using FileUploadService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FileUploadService.Process
{
    public interface IFileUploadService
    {
        Task<HttpResponseMessage> UploadFile(string appId, object data, string metaData, CancellationToken cancellationToken);
    }
    public class FileUploadService : IFileUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly MetaInfo _metaInfo;
        private readonly IEnumerable<VendorInformation> _vendorsInformation;
        private readonly SchemaValidator _schemaValidator;
        private readonly ILogger<FileUploadService> _logger;
        public FileUploadService(IHttpClientFactory httpClientFactory, MetaInfo metaInfo, IEnumerable<VendorInformation> vendorsInformation
            , SchemaValidator schemaValidator, ILogger<FileUploadService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("StorageAccount");
            _metaInfo = metaInfo;
            _vendorsInformation = vendorsInformation;
            _schemaValidator = schemaValidator;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> UploadFile(string appId, object data, string metaData, CancellationToken cancellationToken)
        {
            _logger.LogInformation("UploadFile Started");
            var appInfo = readAppInformation(appId);
            if (!string.IsNullOrWhiteSpace(appInfo.Schemaname))
            {
                _schemaValidator.SchemaErrorNewtonsoft(appInfo.Schemaname, data.ToString(), cancellationToken);
            }
            var fileName = populateFileName(appInfo, metaData);
            _logger.LogInformation($"Created Filename is {fileName}");

            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, $"file?Path={appInfo.containerName}/{fileName}");
            req.Content = new StringContent(data.ToString());
            var response = await _httpClient.SendAsync(req, cancellationToken);
            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Response: {result}");
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else if (((int)response.StatusCode) == StatusCodes.Status400BadRequest)
            {
                var responseMessage = await response.Content.ReadAsStringAsync();
                throw new BadHttpRequestException(result);
            }
            else
            {
                throw new Exception("Failed to upload");
            }

        }

        private string populateFileName(VendorInformation appInfo, string metaData)
        {
            if (!string.IsNullOrWhiteSpace(metaData))
            {
                try
                {
                    var fileName = appInfo.Filenamestructure;
                    Regex rgX = new Regex(@"{([^{}]+)}");
                    var placeHolders = rgX.Matches(appInfo.Filenamestructure).Select(x => x.Groups[1].Value);
                    var fileNameInfo = metaData.Split(',').Select(x => x.Split('=')).Where(x => x.Count() == 2
                    && placeHolders.Contains(x[0]))
                        .GroupBy(x => x[0]).Select(x => new { x.Key, Value = x.Select(y => y[1]).First() });
                    if (placeHolders.Count() != fileNameInfo.Count())
                    {
                        throw new BadHttpRequestException("Missing MetaData");
                    }
                    foreach (var x in fileNameInfo)
                    {
                        fileName = fileName.Replace("{" + x.Key + "}", x.Value);
                    }
                    return fileName;

                }
                catch (Exception ex)
                {
                    throw new BadHttpRequestException("Invalid MetaData");
                }
            }
            return appInfo.Filenamestructure;
        }

        private VendorInformation readAppInformation(string appId)
        {
            var appInfo = _vendorsInformation.FirstOrDefault(x => string.Equals(x.AppID, appId, StringComparison.OrdinalIgnoreCase));

            if (appInfo == null)
            {
                throw new BadHttpRequestException("Invalid AppId");
            }
            return appInfo;
        }
    }
}
