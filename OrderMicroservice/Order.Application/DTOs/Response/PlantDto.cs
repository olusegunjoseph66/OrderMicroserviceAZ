using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class PlantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime DateRefreshed { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string PlantTypeCode { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}
