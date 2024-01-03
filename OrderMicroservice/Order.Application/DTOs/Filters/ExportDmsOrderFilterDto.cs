using Order.Application.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Filters
{
    public class ExportDmsOrderFilterDto
    {
        public string CountryCode { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderStatusCode { get; set; }
        public string CompanyCode { get; set; }
        public string SearchKeyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsATC { get; set; }
        [Required]
        public OrderSortingEnum Sort { get; set; }
        [Required(ErrorMessage = "Format")]
        public ExportFileTypeEnum? Format { get; set; }
    }
}
