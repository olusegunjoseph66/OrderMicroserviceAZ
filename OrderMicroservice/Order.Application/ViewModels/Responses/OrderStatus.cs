using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class OrderStatus
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class OrderStatusResponse
    {
        public List<OrderStatus> orderStatuses { get; set; }
    }
}
