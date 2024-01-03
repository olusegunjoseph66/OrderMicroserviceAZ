using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public record PlantResponse
    {
        public int PlantId { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public PlantTypeResponse PlantType { get; set; }

        public PlantResponse(int id, string companyCode, string countryCode, string name, PlantTypeResponse status, string code)
        {
            PlantId = id;
            CompanyCode = companyCode;
            CountryCode = countryCode;
            Code = code;
            Name = name;
            PlantType = status;
        }
    }
}
