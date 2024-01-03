using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class ProductDto
    {
        
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ProductSapNumber { get; set; } = null!;
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string UnitOfMeasureCode { get; set; } = null!;
        public DateTime DateRefreshed { get; set; }
        public decimal Price { get; set; }
    }
}
