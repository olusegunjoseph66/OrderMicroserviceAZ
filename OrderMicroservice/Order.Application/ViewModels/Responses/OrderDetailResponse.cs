using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class OrderDetailResponse
    {
        public int DmsOrderId { get; set; }
        public string ChangeType { get; set; }
        public string OldOrderStatus { get; set; }
        public string NewOrderStatus { get; set; }
        public DateTime? NewDateModified { get; set; }        
        

      
    }
}
