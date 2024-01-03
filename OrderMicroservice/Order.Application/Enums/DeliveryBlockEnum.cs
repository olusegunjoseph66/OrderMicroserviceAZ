using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Enums
{
    public enum DeliveryBlockEnum
    {
        [Description("Open")]
        Open = 1,

        [Description("Close")]
        Close = 2,

        
    }
}
