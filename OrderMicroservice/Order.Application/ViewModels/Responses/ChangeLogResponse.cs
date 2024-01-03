using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class ChangeLogResponse
    {
        public int dmsOrderId { get; set; }
        public string ChangeType { get; set; }
        public DateTime? NewDateModified { get; set; }
        public OrderStatus OldOrderStatus { get; set; }
        public OrderStatus NewOrderStatus { get; set; }
    }

    public class dmsOrderChangeLogResponse
    {
        public List<ChangeLogResponse> dmsOrderChangeLog { get; set; }
    }
}
