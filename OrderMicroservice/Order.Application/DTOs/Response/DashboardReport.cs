using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Response
{
    public class DashboardReport
    {
        public distributorSapAccount1 distributorSapAccount { get; set; }
        public DashboardMetric Metrics { get; set; }
        public List<DashBoardOrders> RecentOrders { get; set; }
    }
    public class DashboardReport2
    {
        public DashboardMetric Metrics { get; set; }
        public List<DashBoardOrders> RecentOrders { get; set; }
    }
    public class DashboardMetric
    {
        public int NumNew { get; set; }
        public int NumSaved { get; set; }
        public int NumSubmitted { get; set; }
    }

    public class DashBoardOrders
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public Shared.ExternalServices.DTOs.APIModels.Status OrderType { get; set; } = null;
        public Shared.ExternalServices.DTOs.APIModels.Status OrderStatus { get; set; } = null;
        public decimal Netvalue { get; set; }
        public int NumItems { get; set; }
    }

    public class distributorSapAccount1
    {
        public int distributorSapAccountId { get; set; }
        public string distributorNumber { get; set; }
        public string distributorName { get; set; }
        public string companyCode { get; set; }
    }
}
