using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class ShoppingCartLimitExceededMessage
    {
        public ShoppingCartLimitExceededMessage()
        {
            ShoppingCartItems = new List<ShoppingCartItemsDto>();
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public int DistributorSapAccountId { get; set; }
        public string DistributorSapNumber { get; set; }
        public string DistributorName { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public decimal AvailableBalance { get; set; }
        public List<ShoppingCartItemsDto> ShoppingCartItems { get; set; }
    }

    public class ShoppingCartItemsDto
    {
        public ShoppingCartItemsDto()
        {
            Product = new ProductMsgDto();
        }
        public int ShoppingCartItemId { get; set; }
        public DateTime DateCreated { get; set; }
        public ProductMsgDto Product { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasureCode { get; set; }
        public decimal SapEstimatedOrderValue { get; set; }
    }

    public class ProductMsgDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
