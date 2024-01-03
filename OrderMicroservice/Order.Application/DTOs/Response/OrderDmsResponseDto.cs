using Shared.Data.Models;
using Shared.ExternalServices.DTOs.APIModels;
using Shared.ExternalServices.ViewModels.Response;
using System.ComponentModel;

namespace Order.Application.DTOs.Response
{
    public class GetMyDMSOrderDto
    {
        [Description("dmsorderId")]
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public OrderStatusDto OrderStatus { get; set; }
        public OrderTypeDto OrderType { get; set; }
        public DeliveryStatusDto DeliveryStatus { get; set; }
        public bool IsAtc { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        public decimal? OrderSapNetValue { get; set; }
        public int numItems { get; set; }

    }

    public class GetAdminDMSOrderDto
    {
        [Description("dmsorderId")]
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string OrderSapNumber { get; set; }
        public string ParentOrderSapNumber { get; set; } = null!;
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public OrderStatusDto OrderStatus { get; set; }
        public OrderTypeDto OrderType { get; set; }
        public DeliveryStatusDto DeliveryStatus { get; set; }
        public DistributorSapAccountDto distributorSapAccount { get; set; }
        public bool IsAtc { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        public decimal? OrderSapNetValue { get; set; }
        public int numItems { get; set; }
        public int UserId { get; set; }

    }
    public class GetMySAPOrderDto
    {
        public int Id { get; set; }
        [Description("netvalue")]
        public decimal? EstimateNetValue { get; set; }
        public DateTime DateCreated { get; set; }
        public int numItems { get; set; }
        public OrderStatusDto OrderStatus { get; set; }
        public OrderTypeDto OrderType { get; set; }
    }

    public class GetMySAPChildOrderDto
    {
        public int Id { get; set; }
        [Description("netvalue")]
        public decimal? EstimateNetValue { get; set; }
        public string ParentId { get; set; }
        public DateTime DateCreated { get; set; }
        public int numItems { get; set; }
        public int distributorNumber { get; set; }
        public OrderStatusDto OrderStatus { get; set; }
        public DeliveryStatusDto DeliveryStatus { get; set; }
        public OrderTypeDto OrderType { get; set; }
    }


    public class ViewDmsOrderHistoryVm
    {
        public ViewDmsOrderHistory dmsOrderChangeLog { get; set; }
    }
    public class ViewDmsOrderHistory
    {
        [Description("dmsOrderId")]
        public int Id { get; set; }
        public decimal? ChangeType { get; set; }
        public DateTime NewDateModified { get; set; }
        public OrderStatusDto OldOrderStatus { get; set; }
        public OrderStatusDto NewOrderStatus { get; set; }
    }
    public class RedisOrderDmsResponseDto
    {
        public List<DmsOrder> Orders { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ViewDmsOrderDetailsDto
    {
        [Description("dmsorderId")]
        public int Id { get; set; }
        public int? DmsOrderGroupId { get; set; }
        public DateTime DateCreated { get; set; }
        public string OrderSapNumber { get; set; }
        public string ParentOrderSapNumber { get; set; }
        public bool IsAtc { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public int DistributorSapAccountId { get; set; }
        public byte OrderStatusId { get; set; }
        public byte OrderTypeId { get; set; }
        public int ShoppingCartId { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        public decimal? OrderSapNetValue { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string TruckSizeCode { get; set; }
        public string DeliveryCity { get; set; }
        public string PlantCode { get; set; }
        public decimal? SapVat { get; set; }
        public decimal? SapFreightCharges { get; set; }
        public string? CustomerPaymentReference { get; set; }
        public DateTime? CustomerPaymentDate { get; set; }
        public DistributorSapAccountDto DistributorSapAccount { get; set; }
        public OrderStatusDto OrderStatus { get; set; } = null!;
        public OrderTypeDto OrderType { get; set; } = null!;
        public DeliveryStatusDto DeliveryStatus { get; set; } = null!;
        public DeliveryStatusDto DeliveryState { get; set; } = null!;
        public CountryResponse DeliveryCountry { get; set; } = null!;
        public DeliveryStatusDto Plant { get; set; } = null!;
        public TruckSizeDto truckSize { get; set; } = null!;
        public SapDelivery delivery { get; set; } = null!;
        public DeliveryMethodDto deliveryMethod { get; set; } = null!;
        public IEnumerable<DmsOrderItemDto> OrderItems { get; set; }
    }
    public class SapDelivery
    {
        public int? Id { get; set; } = 2;
        public DateTime? deliveryDate { get; set; } = DateTime.UtcNow;
        public DateTime? pickUpDate { get; set; } = DateTime.UtcNow;
        public DateTime? loadingDate { get; set; } = DateTime.UtcNow;
        public DateTime? transportDate { get; set; } = DateTime.UtcNow;
        public DateTime? plannedGoodsMovementDate { get; set; } = DateTime.UtcNow;
        public string WayBillNumber { get; set; }
        //public SapTrip trip { get; set; } = new SapTrip();
    }
    public class SapTrip
    {
        public int? Id { get; set; } = 5;
        public DeliveryStatusDto Trip {
            get
            {
                return new DeliveryStatusDto { Code = "X", Name = "Open/Dispatched" };
            }
        } 
    }
    public class ViewDmsOrderDetailsDtoVM
    {
        public ViewDmsOrderDetailsDto sapOrder { get; set; }
    }
    public class ViewSapOrderDetailsDtoVM
    {
        public ViewSapOrderDetailsDto sapOrder { get; set; }
    }

    //Live Response
    public class ViewSapOrderDetailsDto
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string DistributorNumber { get; set; }
        public string ParentId { get; set; }
        public decimal? NetValue { get; set; }
        public SApStatusResponse OrderType { get; set; } = null!;
        public SApStatusResponse Status { get; set; } = null!;
        public SApStatusResponse DeliveryStatus { get; set; } = null!;
        public SApStatusResponse DeliveryMethod { get; set; } = null!;
        public SApStatusResponse DeliveryBlock { get; set; } = null!;
        public string Reference { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        //public decimal? OrderSapNetValue { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryAddress { get; set; }
        public decimal? Vat { get; set; }
        public decimal SapFreightCharges { get; set; }
        public SApStatusResponse Trucksize { get; set; } = null!;
        public SapDelivery Delivery { get; set; }
        public IEnumerable<OrderResponseDto> OrderItems { get; set; }
    }


    public class OrderResponseDto
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public ProductSapDt0 Product { get; set; }
        public string OrderQuantity { get; set; }
        public string DeliveryQuantity { get; set; }
        public SApStatusResponse SalesUnitOfMeasure { get; set; }
        public SApStatusResponse Plant { get; set; }
        public SApStatusResponse ShippingPoint { get; set; }
        public double PricePerUnit { get; set; }
        public double NetValue { get; set; }
    }

    public class ProductSapDt0
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string ProductType { get; set; }

    }
    public class SApStatusResponse
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class SapOrderDmsAndDistributorResponseDto
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string OrderSapNumber { get; set; }
        public string ParentOrderSapNumber { get; set; }
        public bool IsAtc { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public int DistributorSapAccountId { get; set; }
        public byte OrderStatusId { get; set; }
        public byte OrderTypeId { get; set; }
        public int ShoppingCartId { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        public decimal? OrderSapNetValue { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string TruckSizeCode { get; set; }
        public string DeliveryCity { get; set; }
        public string PlantCode { get; set; }
        public decimal? SapVat { get; set; }
        public decimal? SapFreightCharges { get; set; }
        public DistributorSapAccountDto DistributorSapAccount { get; set; }
        public OrderStatusDto OrderStatus { get; set; } = null!;
        public OrderType OrderType { get; set; } = null!;
        public DeliveryStatusDto DeliveryStatus { get; set; } = null!;
        public IEnumerable<DmsOrderItemDto> DmsOrderItems { get; set; }
    }
}
