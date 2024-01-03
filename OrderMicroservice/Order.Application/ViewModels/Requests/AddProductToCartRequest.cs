using System.ComponentModel.DataAnnotations;

namespace Order.Application.ViewModels.Requests;

public class AddProductToCartRequest
{
    public int ProductId { get; set; }
    public int DistributorSapAccountId { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public string ChannelCode { get; set; }
}

public class AddProductToCartRequestV2
{
    [Required]
    public int ProductId { get; set; }
    [Required]
    public double Quantity { get; set; }
    [Required]
    public string UnitOfMeasureCode { get; set; }
    [Required]
    public string DistributorSapNumber { get; set; }
    [Required]
    public string ChannelCode { get; set; }
    [Required]
    public string PlantCode { get; set; }
    [Required]
    public string DeliveryMethodCode { get; set; }
    public string CompanyCode { get; set; }
    public string CountryCode { get; set; }
    public string DeliveryCountryCode { get; set; }
    public string DeliveryStateCode { get; set; }
}
