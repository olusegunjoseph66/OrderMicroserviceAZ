using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class ShoppingCartItemsResponse
    {
        public int ShoppingCartItemId { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasureCode { get; set; }
        public ProductResponse Product { get; set; }
        public DistributorSapAccountResponse  DistributorSapAccount { get; set; }

       
    }

    public class ShoppingCartItemsResponse2
    {
        public int ShoppingCartItemId { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasureCode { get; set; }
        public CartProductResponse Product { get; set; }
        public decimal estimatedValue { get; set; } = 0;
        public DistributorCartResponse DistributorSapAccount { get; set; }
        public string PlantCode { get; set; }
        public string PlantTypeCode { get; set; }
        public string DeliveryMethodCode { get; set; }
        public decimal SapEstimatedOrderValue { get; set; }
        public string DeliveryCountryCode { get; set; }
        public string DeliveryStateCode { get; set; }
        
    }
}
