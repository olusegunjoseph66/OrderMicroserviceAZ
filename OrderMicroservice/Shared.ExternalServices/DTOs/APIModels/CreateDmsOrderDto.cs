using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.DTOs.APIModels
{


    public class CreateDmsOrderDto
    {
        public string distributorNumber { get; set; }
        public string companyCode { get; set; }
        public OrderitemDto orderItem { get; set; }
        public string plantCode { get; set; }
        public string deliveryMethodCode { get; set; }
        public string customerPaymentReference { get; set; }
        public string customerPaymentDate { get; set; }
        public string deliveryDate { get; set; }
        public string deliveryAddress { get; set; }
        public string deliveryStateCode { get; set; }
        public string truckSizeCode { get; set; }
        //public string deliveryCity { get; set; }
    }

    public class OrderitemDto
    {
        public string productId { get; set; }
        public string quantity { get; set; }
        public string unitOfMeasureCode { get; set; }
    }


    public class CreateDmsOrderItemDto
    {
        public string ProductId { get; set; }
        public string unitOfMeasureCode { get; set; }
        public decimal? Quantity { get; set; }
    }


    public class CreateDmsOrderDelivery
    {
        public string OrderId { get; set; }
        public string companyCode { get; set; }
        public string deliveryCity { get; set; }
        public string deliveryAddress { get; set; }
        public string plantCode { get; set; }
        public string deliveryDate { get; set; }
    }

}
