using System.Collections.Generic;

namespace FileUploadService
{

    public class Vendors
    {
        public IEnumerable<VendorInformation> Vendor { get; set; }
    }

    public class VendorInformation
    {
        public string Client { get; set; }
        public string AppID { get; set; }
        public string Filenamestructure { get; set; }
        public string Storagefoldername { get; set; }
        public string Schemaname { get; set; }
        public string containerName { get; set; }
    }
}