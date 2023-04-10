using FileUploadService.Models;
using FileUploadService.Process;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FileUploadService.Controllers
{
    [Produces("application/json")]

    [ApiController]
    public class FileUploadClientController : ControllerBase
    {

        private readonly ILogger<FileUploadClientController> _logger;
        private readonly IConfiguration Config;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileUploadService _fileUploadService;
        public FileUploadClientController(ILogger<FileUploadClientController> logger
            , IConfiguration _Config
            , IWebHostEnvironment webHostEnvironment
            , IFileUploadService fileUploadService)
        {
            _logger = logger;
            Config = _Config;
            _webHostEnvironment = webHostEnvironment;
            _fileUploadService = fileUploadService;
        }
        public enum ErrorCode
        {
            AppIDInValid = 204,
            FileUploadSuccess = 200,
            FileMissing = 400,
            InvalidFileName = 400,
            InvalidJsonSyntax = 400,
            InvalidJsonSchema = 400,
            InvalidFolderPath = 400,
            InvalidFileContent = 400,
            FileUploadFailure = 400
        }

        /// <summary>
        /// Upload a file
        /// </summary>        
        /// <returns>Response Message</returns>
        /// <remarks>        
        /// Sample Request :
        /// /v1/{appID}/
        /// </remarks>
        [HttpPost]
        [EnableCors("AllowAllHeaders")]
        [Route("file/{appID}")]
        [ProducesDefaultResponseType(typeof(JsonResponseData))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(OperationId = "services-upload-file", Summary = "File upload",
            Description = "Upload an application-specific file to Ingka Centres")]
        public async Task<IActionResult> ProcessFileAsync([FromRoute, SwaggerParameter("The ID of the application uploading the file.", Required = true)] string appID,
                        [FromQuery, SwaggerParameter("The file metadata, used in the data processing logic.")] string metaData)
        {
            _logger.Log(LogLevel.Information, "ProcessFileAsync started");
            if (!Request.Form.Files.Any())
            {
                return BadRequest("No file content.");
            }
            return Ok();
        }



        /// <summary>
        /// Upload a file
        /// </summary>        
        /// <returns>Response Message</returns>
        /// <remarks>        
        /// Sample Request :
        /// /v1/{appID}/
        /// </remarks>
        [HttpPost]
        [EnableCors("AllowAllHeaders")]
        [Route("data/{appID}")]
        [ProducesDefaultResponseType(typeof(JsonResponseData))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(OperationId = "services-upload-file", Summary = "File upload",
            Description = "Upload an application-specific file to Ingka Centres")]
        public async Task<IActionResult> processDataAsync([FromRoute, SwaggerParameter("The ID of the application uploading the file.", Required = true)] string appID,
            [FromQuery, SwaggerParameter("The file metadata, used in the data processing logic.")] string metaData,
            [FromBody, SwaggerRequestBody("data")] object data)
        {
            try
            {
                await _fileUploadService.UploadFile(appID, data, metaData,CancellationToken.None);
                return Ok();
            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
