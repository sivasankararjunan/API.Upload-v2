using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace FileUploadService.Models
{
    public class MetaInfo
    {
        public BlobInfo BlobInfo { get; set; }
        public AzureStorage AzureStorage { get; set; }
        public KeyVault KeyVault { get; set; }

    }
    public class BlobInfo
    {
        public string Connectstring { get; set; }
        public string StorageType { get; set; }
    }

    public class AzureStorage
    {
        public string BaseUrl { get; set; }
        public string UploadFile { get; set; }
    }

    public class KeyVault
    {
        public string Url { get; set; }
        public string ConnectionString { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}