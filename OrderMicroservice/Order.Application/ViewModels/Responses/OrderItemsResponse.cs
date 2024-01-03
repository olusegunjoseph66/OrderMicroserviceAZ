using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class OrderItemsResponse
    {
        public int DmsOrderItemId { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal Quantity { get; set; }
        public string SalesUnitOfMeasureCode { get; set; }
        public ProductResponse Product { get; set; }


    }

    public class FailSubmissionOrderItemsResponse
    {
        public int DmsOrderItemId { get; set; }
        public decimal Quantity { get; set; }
        public string SalesUnitOfMeasureCode { get; set; }
        public ProductResponse Product { get; set; }


    }
}
