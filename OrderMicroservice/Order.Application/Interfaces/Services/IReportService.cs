using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<ApiResponse> GetMyReportByProduct(ProductReportRequestVM productReport);
    }
}
