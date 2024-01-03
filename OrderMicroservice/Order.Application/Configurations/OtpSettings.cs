using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Configurations
{
    public class OtpSettings
    {
        public byte MaximumRequiredRetries { get; set; }
        public byte RetryIntervalInMinutes { get; set; }
        public byte OtpCodeLength { get; set; }
        public byte OtpExpiryInMinutes { get; set; }
    }
}
