using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadService.Models
{
    public class ErrorLog
    {
        public string FileName { get; set; }
        public string FileContent { get; set; }
        public string MeetingPlace { get; set; }
        public string Country { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ErrorLogDate { get; set; }
        public string Guidelines { get; set; }
    }
}
