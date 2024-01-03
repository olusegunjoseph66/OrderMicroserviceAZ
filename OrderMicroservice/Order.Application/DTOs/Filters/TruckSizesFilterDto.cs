using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Filters
{
    public class TruckSizesFilterDto
    {
        public string DeliveryMethodCode { get; set; }
       
        public string PlantTypeCode { get; set; }
    }
}
