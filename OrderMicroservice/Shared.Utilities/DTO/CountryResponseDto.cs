using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs
{


    public class Country
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Data
    {
        public Data data { get; set; }
        public string status { get; set; }
        public string statusCode { get; set; }
        public string message { get; set; }
        public List<Country> countries { get; set; }
    }

    public class CountryResponseDto
    {
        public bool success { get; set; }
        public Data data { get; set; }
    }


}
