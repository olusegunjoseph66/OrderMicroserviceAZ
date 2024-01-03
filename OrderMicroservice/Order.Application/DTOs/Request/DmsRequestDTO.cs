using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Request
{
    public class DmsRequestDTO
    {
        public int? DmsOrderId { get; set; }
        public int? DmsOrderGroupId { get; set; }
        [Required]
        public string TruckSizeCode { get; set; }
        [Required]
        public string DeliveryMethodCode { get; set; }
        [Required]
        public string PlantCode { get; set; }
        [Required]
        public DateTime DeliveryDate { get; set; }
        [Required]
        public string DeliveryCity { get; set; }
        [Required]
        public string DeliveryStateCode { get; set; }
        [Required]
        public string DeliveryAddress { get; set; }
        [Required]
        public string DeliveryCountryCode { get; set; }
        [Required]
        public string ChannelCode { get; set; }
    }

    public class DmsRequestDTOV2
    {
        public int? DmsOrderGroupId { get; set; }
        public string TruckSizeCode { get; set; }
       
        [Required]
        public DateTime DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        [Required]
        public string ChannelCode { get; set; }
        public string CustomerPaymentReference { get; set; }
        public DateTime CustomerPaymentDate { get; set; }
    }


    public class CancelDmsRequestDTO
    {
        public int DmsOrderGroupId { get; set; }
        public int DmsOrderId { get; set; }
        public string orderSapNumber { get; set; }
        public int distributorSapAccountId { get; set; }
    }
}
