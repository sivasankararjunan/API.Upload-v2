using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using NJsonSchema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace API.Upload_v2.Utilities
{
    public static class Utils
    {
        public static string AsJson(this IEnumerable input)
        {
            return JsonConvert.SerializeObject(input);
        }
    }


    public interface IHttpUtilities
    {
        object ReadFile(HttpRequest httpRequest);
    }
    public class HttpUtilities : IHttpUtilities
    {
        public object ReadFile(HttpRequest httpRequest)
        {
            if (httpRequest.Form.Files == null || !httpRequest.Form.Files.Any())
            {
                throw new BadHttpRequestException("No file content.");
            }

            using (var stream = httpRequest.Form.Files.First().OpenReadStream())
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
