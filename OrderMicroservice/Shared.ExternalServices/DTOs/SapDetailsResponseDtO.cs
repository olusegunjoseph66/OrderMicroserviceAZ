using Shared.ExternalServices.DTOs.APIModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.DTOs
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Data12
    {
        public SapOrder1 sapOrder { get; set; }
    }


    public class SapDetailsResponse
    {
        public string Id { get; set; }
        public string parentId { get; set; }
        public string distributorNumber { get; set; }
        public DateTime dateCreated { get; set; }
        public Status orderType { get; set; }
        public Status orderStatus { get; set; }
        public Status deliveryStatus { get; set; }
        public Status deliveryBlock { get; set; }
        public decimal? netvalue { get; set; }
        public DateTime? deliveryDate { get; set; }
        public Status trucksize { get; set; }
        public string reference { get; set; }
        public string deliveryAddress { get; set; }
        public string deliveryCity { get; set; }
        public decimal vat { get; set; }
        public string freightCharges { get; set; }
        public SapDelivery delivery { get; set; }
        public OrderItems orderItems { get; set; }
    }

    public class Delivery
    {
        public long Id { get; set; }
        public string deliveryDate { get; set; }
        public string pickUpDate { get; set; }
        public string loadingDate { get; set; }
        public string transportDate { get; set; }
        public string plannedGoodsMovementDate { get; set; }
        public string WayBillNumber { get; set; }
    }

    public class DeliveryBlock
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class DeliveryMethod
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class DeliveryStatus
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class OrderItems
    {
        public long Id { get; set; }
        public long orderId { get; set; }
        public Product product { get; set; }
        public string orderQuantity { get; set; }
        public string deliveryQuantity { get; set; }
        public SalesUnitOfMeasure salesUnitOfMeasure { get; set; }
        public Status plant { get; set; }
        public DeliveryStatus shippingPoint { get; set; }
        public string pricePerUnit { get; set; }
        public string netValue { get; set; }
    }

    
 


    public class Product
    {
        public string productId { get; set; }
        public string name { get; set; }
        public string productType { get; set; }
    }

    public class APIResponseDto
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }

    public class Root
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public Data12 data { get; set; }
    }

    public class SalesUnitOfMeasure
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class SapOrder1
    {
        public long Id { get; set; }
        public long parentId { get; set; }
        public int distirbutorNumber { get; set; }
        public string dateCreated { get; set; }
        public string netValue { get; set; }
        public Status orderType { get; set; }
        public Status Status { get; set; }
        public DeliveryStatus deliveryStatus { get; set; }
        public DeliveryMethod deliveryMethod { get; set; }
        public DeliveryBlock deliveryBlock { get; set; }
        public DeliveryBlock creditBlock { get; set; }
        public string reference { get; set; }
        public string deliveryAddress { get; set; }
        public string deliveryCity { get; set; }
        public string vat { get; set; }
        public string freightCharges { get; set; }
        public Delivery delivery { get; set; }
        public List<OrderItems> orderItems { get; set; }
    }

 


}
