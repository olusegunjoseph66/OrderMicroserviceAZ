using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Requests
{
    public class RequestReportByProductVm
    {
        public string distributorNumber { get; set; }
        public string companyCode { get; set; }
        public string countryCode { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
    }

    public class RequestReportByProductViewModel
    {
        [Required]
        public int distributorSapAccountId { get; set; }
        public int productId { get; set; }
        [Required]
        public DateTime fromDate { get; set; }
        [Required]
        public DateTime toDate { get; set; }
        [Required]
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        [Required]
        public ReportSortingEnum sort { get; set; }
    }


    public enum ReportSortingEnum
    {
        [Description("date_asc")]
        DateAscending = 1,

        [Description("date_desc")]
        DateDescending = 2,

        [Description("value_desc")]
        ValueDescending = 3,

        [Description("value_asc")]
        ValueAscending = 4,
    }
}
