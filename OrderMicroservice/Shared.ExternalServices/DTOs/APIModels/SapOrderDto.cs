using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.DTOs.APIModels
{
    public class SapOrderDto
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public DateTime DateCreated { get; set; }
        public string DistributorNumber { get; set; }
        public string NumberOfItems { get; set; }
        public string NetValue { get; set; }
        public string deliveryAddress { get; set; }
        public string deliveryDate { get; set; }
        public Status Status { get; set; }
        public Status OrderType { get; set; }
        public Status DeliveryBlock { get; set; }
    }

    public partial class SapChildrenResponseDTO
    {
        public string StatusCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public Data Data { get; set; }
    }

    public partial class Data
    {
       public List<SapOrderDto> SapOrders { get; set; }
    }
}
