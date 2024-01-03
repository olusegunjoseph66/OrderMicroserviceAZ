using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class ProductResponse
    {
        //public ProductResponse(int productId,  string name)
        //{
        //    ProductId = productId;
        //    Name = name;
        //}
        public ProductResponse(int productId, decimal price, string name)
        {
            ProductId = productId;
            Price = price;
            Name = name;
        }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; }
    }

    public class CartProductResponse
    {
       
        public int ProductId { get; set; }
        public string Name { get; set; }
    }


}
