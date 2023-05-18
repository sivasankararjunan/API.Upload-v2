using API.Upload_v2.Utilities;
using FileUploadService.Models;
using FileUploadService.Process;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileUploadService.Controllers
{
    [Produces("application/json")]

    [ApiController]
    public class FileUploadController : ControllerBase
    {

        private readonly ILogger<FileUploadController> _logger;
        private readonly IConfiguration Config;
        private readonly IFileUploadService _fileUploadService;
        private readonly IHttpUtilities _httpUtilities;
        public FileUploadController(ILogger<FileUploadController> logger
            , IConfiguration _Config
            , IFileUploadService fileUploadService
            , IHttpUtilities httpUtilities)
        {
            _logger = logger;
            Config = _Config;
            _fileUploadService = fileUploadService;
            _httpUtilities = httpUtilities;
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
            try
            {
                _logger.Log(LogLevel.Information, "ProcessFileAsync started");
                var data = _httpUtilities.ReadFile(Request);
                await _fileUploadService.UploadFile(appID, data, metaData, CancellationToken.None);

                return Ok(new { Status_code = StatusCodes.Status201Created, Message = "File uploaded!" });
            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest(new
                {
                    Status_code = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status_code = StatusCodes.Status500InternalServerError, Message = "Upload failed" });
            }
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
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow), SwaggerRequestBody("data")] object data)
        {
            if (data is null || string.IsNullOrWhiteSpace(Convert.ToString(data)))
            {
                return BadRequest(new { Status_code = StatusCodes.Status400BadRequest, Message = "A non-empty request body is required." });
            }
            try
            {
                await _fileUploadService.UploadFile(appID, data, metaData, CancellationToken.None);
                return Ok(new { Status_code = StatusCodes.Status201Created, Message = "File uploaded!" });
            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest(new { Status_code = StatusCodes.Status400BadRequest, ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status_code = StatusCodes.Status500InternalServerError, Message = "Upload failed" });
            }
        }

    }
}
