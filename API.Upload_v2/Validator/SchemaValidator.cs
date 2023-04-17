using API.Upload_v2.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using NJsonSchema;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace API.Upload_v2.Validator
{
    public class SchemaValidator
    {
        private readonly ILogger<SchemaValidator> _logger;
        public SchemaValidator(ILogger<SchemaValidator> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Schema Validation
        /// </summary>
        /// <param name="ValidatorFile"></param>
        /// <param name="Data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SchemaError(string ValidatorFile, string Data, CancellationToken cancellationToken)
        {
            var schema = await NJsonSchema.JsonSchema.FromFileAsync(ValidatorFile, cancellationToken);
            var validationError = schema.Validate(Data);
            _logger.LogInformation($"Schema Validation Error : {JsonConvert.SerializeObject(validationError)}");

            if (validationError.Any())
            {
                throw new BadHttpRequestException(validationError.Select(x => new { Kind = x.Kind.ToString(), x.Property }).AsJson());
            }
        }
    }
}
