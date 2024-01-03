using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.DTOs.APIModels
{
 
    public class SapSearchChildOrderDto1
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public DateTime DateCreated { get; set; }
        public string DistributorNumber { get; set; }
        public string NumberOfItems { get; set; }
        public string NetValue { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryDate { get; set; }
        public Status Status { get; set; }
        public Status OrderType { get; set; }
        public Status DeliveryBlock { get; set; }
    }

    public partial class SapSearchChildOrderDto
    {
        public string StatusCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public SearchData Data { get; set; }
    }

    public partial class SearchData
    {
        public SapSearchChildOrderDto1 SapOrders { get; set; }
    }
}
