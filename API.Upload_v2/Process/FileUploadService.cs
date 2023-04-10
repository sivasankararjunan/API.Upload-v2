using FileUploadService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
        private readonly Vendors _vendors;
        public FileUploadService(IHttpClientFactory httpClientFactory, MetaInfo metaInfo, Vendors vendors)
        {
            _httpClient = httpClientFactory.CreateClient("StorageAccount");
            _metaInfo = metaInfo;
            _vendors = vendors;
        }

        public async Task<HttpResponseMessage> UploadFile(string appId, object data, string metaData, CancellationToken cancellationToken)
        {

            var appInfo = readAppInformation(appId);
            var fileName = populateFileName(appInfo, metaData);
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, $"file?Path={appInfo.containerName}/{fileName}");
            req.Content = new StringContent(data.ToString());
            var response = await _httpClient.SendAsync(req, cancellationToken);

            return response;
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
            var appInfo = _vendors.Vendor.FirstOrDefault(x => string.Equals(x.AppID, appId, StringComparison.OrdinalIgnoreCase));

            if (appInfo == null)
            {
                throw new BadHttpRequestException("Invalid AppId");
            }
            return appInfo;
        }
    }
}
