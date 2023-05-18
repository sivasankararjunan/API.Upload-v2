using API.Upload_v2.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NJsonSchema.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace API.Upload_v2.Validator
{
    public interface ISchemaValidator
    {
        Task SchemaErrorNJson(string ValidatorFile, string Data, CancellationToken cancellationToken);
        void SchemaErrorNewtonsoft(string ValidatorFile, string Data);
    }
    public class SchemaValidator : ISchemaValidator
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
        public async Task SchemaErrorNJson(string ValidatorFile, string Data, CancellationToken cancellationToken)
        {
            var schema = await NJsonSchema.JsonSchema.FromFileAsync(ValidatorFile, cancellationToken);
            ICollection<NJsonSchema.Validation.ValidationError> validationError = schema.Validate(Data);

            if (validationError.Any())
            {
                _logger.LogInformation($"Schema Validation Error : {JsonConvert.SerializeObject(validationError)}");
                throw new BadHttpRequestException(string.Join(',', validationError.Select(x => Enum.GetName(typeof(ValidationErrorKind), x.Kind))));
            }
        }

        /// <summary>
        /// Schema Validation
        /// </summary>
        /// <param name="ValidatorFile"></param>
        /// <param name="Data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public void SchemaErrorNewtonsoft(string ValidatorFile, string Data)
        {
            var schema = JSchema.Parse(File.ReadAllText(ValidatorFile));
            var dataObject = JToken.Parse(Data);
            IList<string> validationError = new List<string>();
            var resx = dataObject.IsValid(schema, out validationError);
            if (!resx)
            {
                _logger.LogInformation($"Schema Validation Error : {JsonConvert.SerializeObject(validationError)}");
                throw new BadHttpRequestException(validationError.AsJson());
            }
        }

        //public string SchemaError2(string ValidatorFile, string Data)
        //{

        //}
    }
}
