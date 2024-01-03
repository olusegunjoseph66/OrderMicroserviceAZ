using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class ProductRefreshedMessage
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasureCode { get; set; }
        public string ProductSapNumber { get; set; }
        public DateTime DateRefreshed { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public decimal Price { get; set; }
        public NameAndCode ProductStatus { get; set; }
    }
    public class NameAndCode
    {
        public NameAndCode(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public string Code { get; set; }
        public string Name { get; set; }
    }
}
