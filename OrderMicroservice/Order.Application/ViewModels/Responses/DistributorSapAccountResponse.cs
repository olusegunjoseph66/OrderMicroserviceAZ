using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class DistributorSapAccountResponse
    {
        public DistributorSapAccountResponse(int Id, string distributorSapNumber, string name)
        {
            DistributorSapAccountId = Id;
            DistributorSapNumber = distributorSapNumber;
            DistributorName = name;
        }
        public int DistributorSapAccountId { get; set; }
        public string DistributorSapNumber { get; set; }
        public string DistributorName { get; set; }
    }

    public class DistributorCartResponse
    {
        public DistributorCartResponse(int Id, string distributorSapNumber, string name, string companyCode)
        {
            DistributorSapAccountId = Id;
            DistributorSapNumber = distributorSapNumber;
            DistributorName = name;
            CompanyCode = companyCode;
        }
        public int DistributorSapAccountId { get; set; }
        public string DistributorSapNumber { get; set; }
        public string DistributorName { get; set; }
        public string CompanyCode { get; set; }
    }
}
