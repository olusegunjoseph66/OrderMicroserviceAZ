using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Constants
{
    public class AppResponseCode
    {

    }

    public class ResponseCodes
    {
        public const int Success = 200;
        public const int Badrequest = 400;
        public const int RecordNotFound = 404;
        public const int Duplicate = 409;
        public const int InternalError = 500;
    }
}
