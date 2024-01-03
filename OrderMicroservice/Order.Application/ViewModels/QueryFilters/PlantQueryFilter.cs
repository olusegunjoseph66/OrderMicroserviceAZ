using System.ComponentModel.DataAnnotations;

namespace Order.Application.ViewModels.QueryFilters
{
    public class PlantQueryFilter
    {
        [Required]
        public string CountryCode { get; set; }
        [Required]
        public string CompanyCode { get; set; }
    }
}
