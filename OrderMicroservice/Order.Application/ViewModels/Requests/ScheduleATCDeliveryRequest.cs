using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Requests
{
    public class ScheduleATCDeliveryRequest
    {
        [Required]
        public int DistributorSapAccountId { get; set; }
        [Required]
        public string OrderSapNumber { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryStateCode { get; set; }
        public string DeliveryCountryCode { get; set; }
        public string PlantCode { get; set; }
        public string DeliveryMethodCode { get; set; }
        public string TruckSizeCode { get; set; }
        [Required]
        public string ChannelCode { get; set; }
        [Required]
        public DateTime DeliveryDate { get; set; }

    
    }
}
