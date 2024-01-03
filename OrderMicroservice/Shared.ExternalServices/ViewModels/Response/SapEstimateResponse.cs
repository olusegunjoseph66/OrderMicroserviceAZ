using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.ViewModels.Response
{
    public class SapEstimateResponse
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public Estimate data { get; set; }
    }

    public class Estimate
    {
        public Sapestimate sapEstimate { get; set; }
    }

    public class Sapestimate
    {
        public double orderValue { get; set; }
        public double freightCharges { get; set; }
        public double vat { get; set; }
    }

}
