using System.Collections.Generic;

namespace FileUploadService
{
    public class VendorInformation
    {
        public string AppID { get; set; }
        public string Filenamestructure { get; set; }
        public string[] FileNameStructures { get { return Filenamestructure.Split(','); } }
        public string[] Types { get; set; }
        public string Schemaname { get; set; }
        public string containerName { get; set; }
    }
}