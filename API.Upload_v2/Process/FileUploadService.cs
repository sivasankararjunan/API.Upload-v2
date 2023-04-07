using FileUploadService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FileUploadService.Process
{
    public interface IFileUploadService
    {
        Task<bool> UploadFile(CancellationToken cancellationToken);
    }
    public class FileUploadService : IFileUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly MetaInfo _metaInfo;
        private readonly IEnumerable<Vendors> _vendors;
        public FileUploadService(IHttpClientFactory httpClientFactory, MetaInfo metaInfo, IEnumerable<Vendors> vendors)
        {
            _httpClient = httpClientFactory.CreateClient("StorageAccount");
            _metaInfo = metaInfo;
            _vendors = vendors;
        }

        public async Task<bool> UploadFile(CancellationToken cancellationToken)
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "file?Path=uploadstorage1/Readme3.json");
            req.Content = new StringContent("test");
            var response = await _httpClient.SendAsync(req, cancellationToken);
            return false;
        }
    }
}
