using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.ExternalServices.DTOs.SD;

namespace Shared.ExternalServices.DTOs
{
    public class ApiRequest
    {
        public ApiType apiType { get; set; } = ApiType.GET;
        public string url { get; set; }
        public object data { get; set; }
        public string accessToken { get; set; }
    }

    public static class SD
    {
        public static string SapAPIBase { get; set; }
        public static string RDATAAPIBase { get; set; }
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE,
        }
    }
}
