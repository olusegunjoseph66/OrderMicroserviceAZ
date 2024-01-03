using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class ATCAvailableMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int DmsOrderId { get; set; }
        public string OrderSapNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public int UserId { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public distributorSapAccount DistributorSapAccount { get; set; }
        public decimal EstimatedNetValue { get; set; }
        public decimal OrderSapNetValue { get; set; }
        public messageStatus NewOrderStatus { get; set; }
        public messageStatus OrderType { get; set; }
        public int NumberOfChildATC { get; set; }
    }
}
