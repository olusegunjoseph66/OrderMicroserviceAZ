using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.ViewModels.Response
{
    public class SAPWalletResponse
    {
        public decimal? AvailableBalance { get; set; }
        public decimal? NetValue { get; set; }
        public decimal? Vat { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public string DistributorName { get; set; }
        public string DistributorNumber { get; set; }
        public string Status { get; set; }

        public string TrucksizeCode { get; set; }
        public string DeliveryMethodCode { get; set; }
        public string PlantCode { get; set; }
        public int? OrderTypeCode { get; set; }
        public string DeliveryBlockCode { get; set; }
        public string DeliveryStatusCode { get; set; }
        public decimal? SapFreightCharges { get; set; }
        public string DelivaryId { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }
        public ICollection<SapOrderItemResponse> SapItems { get; set; }
        

    }

    public class CountryResponse
    {
        public string code { get; set; }
        public string name { get; set; }

    }
    public class CountryVms
    {
        public CountryVm data { get; set; }
    }
    public class CountryVm
    {
        public List<CountryResponse> countries { get; set; }
    }
    public class StateResponse
    {
        public string CountryCode { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }

    }
    public class SapOrderItemResponse
    {
        public int? Id { get; set; }
        public decimal? PricePerUnit { get; set; }
        public decimal? NetValue { get; set; }
        public decimal? Deliveryquanity { get; set; }

    }
}
