using API.Upload_v2.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            DateTime dt = DateTime.Now;
            var validationError = schema.Validate(Data);
            DateTime dt1 = DateTime.Now;
            _logger.LogInformation($"Schema Validation Error : {JsonConvert.SerializeObject(validationError)}");

            var response = await SchemaError1(ValidatorFile, Data, cancellationToken);
            DateTime dt2 = DateTime.Now;
            if (validationError.Any())
            {
                throw new BadHttpRequestException(validationError.Select(x => new { Kind = x.Kind.ToString(), x.Property }).AsJson());
            }
        }

        /// <summary>
        /// Schema Validation
        /// </summary>
        /// <param name="ValidatorFile"></param>
        /// <param name="Data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IList<string>> SchemaError1(string ValidatorFile, string Data, CancellationToken cancellationToken)
        {
            var schema = JSchema.Parse(File.ReadAllText(ValidatorFile));
            var dataObject = JObject.Parse(Data);
            IList<string> str = new List<string>();
            var resx = dataObject.IsValid(schema, out str);
            return str;
        }

        //public string SchemaError2(string ValidatorFile, string Data)
        //{

        //}
    }
}
