using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class StartCheckResponce
    {
        public StartCheckResponceVM dmsOrder { get; set; }
    }

    public class StartCheckResponceVM
    {
        public int DmsOrderId { get; set; }
    }

}
