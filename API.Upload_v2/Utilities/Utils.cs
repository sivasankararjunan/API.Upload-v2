using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using NJsonSchema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
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
}
