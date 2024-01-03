using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.ViewModels.Response
{
    public class PlantResponse
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime DateRefreshed { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string PlantTypeCode { get; set; } = null!;
        public string Address { get; set; } = null!;
    }



    public class SapPlantResponse
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public Datum data { get; set; }
    }

    public class Datum
    {
        public Sapplant[] sapPlants { get; set; }
    }

    public class Sapplant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

}
