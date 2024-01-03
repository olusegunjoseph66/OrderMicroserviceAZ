using System.ComponentModel.DataAnnotations;

namespace Order.Application.ViewModels.QueryFilters
{
    public class TruckSizesQueryFilter
    {
        [Required]
        public string DeliveryMethodCode { get; set; }
        [Required]
        public string PlantTypeCode { get; set; }
    }
    public class TruckSizesQueryFilterV2
    {
        [Required]
        public string DeliveryMethodCode { get; set; }
        [Required]
        public int DmsOrderGroupId { get; set; }
    }
}
