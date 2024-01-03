using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.DTOs.APIModels
{
    public class ViewSapOrderDetailsDto
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
        public Status OrderStatus { get; set; } = null!;
        public Status OrderType { get; set; } = null!;
        public Status DeliveryStatus { get; set; } = null!;
        public Status DeliveryBlock { get; set; } = null!;
        public SapDelivery delivery { get; set; } = null!;
        public Status deliveryMethod { get; set; } = null!;
        public IEnumerable<DmsOrderItemDto> OrderItems { get; set; }
    }
    public class SapDetailsResponseDto
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public IO data { get; set; }
    }
    public class IO
    {
        public ViewSapOrderDetailsDto SapOrder { get; set; }
    }
    public partial class DmsOrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal? orderQuantity { get; set; }
        public decimal? deliveryQuantity { get; set; }
        public decimal? PricePerUnit { get; set; }
        public decimal? NetValue { get; set; }
        public ProductDto product { get; set; }
        public Status plant { get; set; }
        public Status shippingPoint { get; set; }
        public Status salesUnitOfMeasure { get; set; }
    }
    public class SapDelivery
    {
        public int? Id { get; set; } = 2;
        public DateTime? deliveryDate { get; set; } = DateTime.UtcNow;
        public DateTime? pickUpDate { get; set; } = DateTime.UtcNow;
        public DateTime? loadingDate { get; set; } = DateTime.UtcNow;
        public DateTime? transportDate { get; set; } = DateTime.UtcNow;
        public DateTime? plannedGoodsMovementDate { get; set; } = DateTime.UtcNow;
    }
    public partial class ProductDto
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class SapTrip
    {
        public string Id { get; set; }
        public string DeliveryId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DispatchDate { get; set; }
        public long? OdometerStart { get; set; }
        public long? OdometerEnd { get; set; }
        public string TruckLocation { get; set; }
        public Status TripStatus { get; set; }
    }


    public class SapList
    {
        public List<SapTripRes> sapTrips { get; set; }
    }

    public class SapTripResponse
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public SapList data { get; set; }
    }

    public class SapTripRes
    {
        public string Id { get; set; }
        public long DeliveryId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DispatchDate { get; set; }
        public long OdometerStart { get; set; }
        public long OdometerEnd { get; set; }
        public string TruckLocation { get; set; }
        public TripStatus tripStatus { get; set; }
    }

    public class TripStatus
    {
        public string code { get; set; }
        public string name { get; set; }
    }









}
