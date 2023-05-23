using API.Upload_v2.Validator;
using FileUploadService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NJsonSchema.Generation;
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
        private readonly ISchemaValidator _schemaValidator;
        private readonly ILogger<FileUploadService> _logger;
        private readonly IConfiguration _configuration;

        public FileUploadService(IHttpClientFactory httpClientFactory, MetaInfo metaInfo, IEnumerable<VendorInformation> vendorsInformation
            , ISchemaValidator schemaValidator, ILogger<FileUploadService> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("StorageAccount");
            _metaInfo = metaInfo;
            _vendorsInformation = vendorsInformation;
            _schemaValidator = schemaValidator;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<HttpResponseMessage> UploadFile(string appId, object data, string metaData, CancellationToken cancellationToken)
        {
            _logger.LogInformation("UploadFile Started");
            var appInfo = readAppInformation(appId);

            string schemaName;
            var fileName = populateFileName(appInfo, metaData, out schemaName);
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                if (_configuration["SchemaType"] == "NJSON")
                {
                    await _schemaValidator.SchemaErrorNJson(schemaName, data.ToString(), cancellationToken);
                }
                else
                {
                    _schemaValidator.SchemaErrorNewtonsoft(schemaName, data.ToString());
                }
            }
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

        private string populateFileName(VendorInformation appInfo, string metaData, out string schemaName)
        {
            schemaName = appInfo.Schemaname;
            Regex rgX = new Regex(@"{([^{}]+)}");

            var placeHolders = rgX.Matches(appInfo.Filenamestructure).Select(x => x.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(metaData))
            {
                try
                {
                    var fileName = appInfo.Filenamestructure;
                    var fileNameInfo = metaData.Split(',').Select(x => x.Split(':')).Where(x => x.Count() == 2
                    && placeHolders.Contains(x[0]))
                        .GroupBy(x => x[0]).Select(x => new { x.Key, Value = x.Select(y => y[1]).First() });

                    if (appInfo.Types != null && appInfo.Types.Any())
                    {
                        var type = fileNameInfo.FirstOrDefault(x => x.Key == "type");
                        if (type == null)
                        {
                            throw new BadHttpRequestException($"Missing metadata parameter: type.");
                        }
                        var index = -1;
                        for (var i = 0; i < appInfo.Types.Length; i++)
                        {
                            if (appInfo.Types[i] == type.Value)
                            {
                                index = i; break;
                            }
                        }
                        if (index == -1)
                        {
                            throw new BadHttpRequestException($"Invalid metadata value: type.");
                        }
                        fileName = appInfo.FileNameStructures[index];
                        placeHolders = rgX.Matches(fileName).Select(x => x.Groups[1].Value);
                    }




                    var missingMetaData = placeHolders.Where(x => !fileNameInfo.Any(y => y.Key == x));

                    if (missingMetaData.Any())
                    {
                        throw new BadHttpRequestException($"Missing metadata parameter: {string.Join(',', missingMetaData)}.");
                    }
                    foreach (var x in fileNameInfo)
                    {
                        fileName = fileName.Replace("{" + x.Key + "}", x.Value);
                        if (!string.IsNullOrEmpty(schemaName))
                        {
                            schemaName = schemaName.Replace("{" + x.Key + "}", x.Value);
                        }
                    }
                    return fileName;

                }
                catch (Exception ex)
                {
                    throw new BadHttpRequestException(ex.Message);
                }
            }
            else if (placeHolders.Any())
            {
                throw new BadHttpRequestException("Missing MetaData");
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
