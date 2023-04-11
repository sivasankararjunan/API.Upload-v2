using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        public async Task<bool> SchemaError(string ValidatorFile, string Data, CancellationToken cancellationToken)
        {
            Type type = Type.GetType(ValidatorFile);
            Activator.CreateInstance(type);
            var schema = NJsonSchema.JsonSchema.FromType(type);
            //FromFileAsync(ValidatorFile, cancellationToken);
            var validationError = schema.Validate(Data);
            _logger.LogInformation(string.Join(',', Data));
            return !validationError.Any();
        }
    }
}
