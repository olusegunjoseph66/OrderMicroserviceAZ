using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class DistributorSapAccountDto
    {
        public int DistributorSapAccountId { get; set; }
        public int UserId { get; set; }
        public string DistributorSapNumber { get; set; } = null!;
        public string DistributorName { get; set; } = null!;
        public DateTime DateRefreshed { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string AccountType { get; set; } = null!;
    }
}
