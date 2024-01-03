using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Requests
{
   
    public class ProofOfDelivery
    {
        public string distributorNumber { get; set; }
        public string companyCode { get; set; }
        public string countryCode { get; set; }
        public string atcNumber { get; set; }
    }
    public class OrderDocumentVm
    {
        public string companyCode { get; set; }
        public string countryCode { get; set; }
        public string atcNumber { get; set; }
    }

}
