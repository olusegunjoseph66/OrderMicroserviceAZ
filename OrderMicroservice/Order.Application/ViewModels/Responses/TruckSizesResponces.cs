using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{

    public class TruckSizesResponces
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public TruckSizesResponces(string code,  string name)
        {
            Code = code;
            
            Name = name;
        }
    }

    public class TruckSizesResponcesVm
    {
        public TruckSizesResponces trucksizes { get; set; }
    }
}
