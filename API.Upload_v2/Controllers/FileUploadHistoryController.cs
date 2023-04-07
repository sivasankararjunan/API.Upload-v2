using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FileUploadService.Controllers
{
    [Produces("application/json")]
    [Route("/api/[controller]")]
    [ApiController]
    public class FileUploadHistoryController : ControllerBase
    {
        

        private readonly ILogger<FileUploadHistoryController> _logger;

        public FileUploadHistoryController(ILogger<FileUploadHistoryController> logger)
        {
            _logger = logger;
        }

         
    }
}
