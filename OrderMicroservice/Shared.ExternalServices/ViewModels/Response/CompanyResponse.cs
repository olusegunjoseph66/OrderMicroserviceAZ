using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.ViewModels.Response
{
    public class CompanyResponse
    {
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public string Code { get; set; }
    }


    public class RDataCompanyResponse
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public Comp data { get; set; }
    }

    public class Comp
    {
        public Company[] companies { get; set; }
    }

    public class Company
    {
        public string name { get; set; }
        public string code { get; set; }
    }



}
