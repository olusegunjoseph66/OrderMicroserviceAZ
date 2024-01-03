using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Requests
{
    public class SaveOrderLaterRequest
    {
        public int? DmsOrderGroupId { get; set; }
        public int? DmsOrderId { get; set; }
        public string TruckSizeCode { get; set; }
        public string DeliveryMethodCode { get; set; }
        public string PlantCode { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryStateCode { get; set; }
        public string DeliveryCountryCode { get; set; }
        public DateTime DeliveryDate { get; set; }

    }
    public class SaveOrderLaterRequestV2
    {
        public int? DmsOrderGroupId { get; set; }
        public string TruckSizeCode { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryCity { get; set; }
        public string CustomerPaymentReference { get; set; }
        public DateTime CustomerPaymentDate { get; set; }

    }
}
