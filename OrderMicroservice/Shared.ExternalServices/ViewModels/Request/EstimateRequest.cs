using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.ViewModels.Request
{
    public class EstimateRequest
    {
        public string distributorNumber { get; set; }
        public string companyCode { get; set; }
        public string productId { get; set; }
        public string quantity { get; set; }
        public string countryCode { get; set; }
        public string plantCode { get; set; }
        public string deliveryMethodCode { get; set; }
        public string unitOfMeasureCode { get; set; }
        public string deliveryStateCode { get; set; }
    }

    public class dictionaryObject
    {
        public double orderValue { get; set; }
        public double freightCharges { get; set; }
        public double Vat { get; set; }
    }

}
