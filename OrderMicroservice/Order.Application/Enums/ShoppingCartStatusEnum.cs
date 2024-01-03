using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Enums
{
    public enum ShoppingCartStatusEnum
    {
        [Description("Active")]
        Active = 1,

        [Description("Abandoned")]
        Abandoned = 2,

        [Description("Checked Out")]
        CheckedOut = 3,

        [Description("Processed")]
        Processed = 4,
        


    }
}
