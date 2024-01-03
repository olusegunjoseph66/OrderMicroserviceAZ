using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Enums
{
    public enum AccountTypeEnum
    {
        [Description("Bank Guarantee")]
        BG = 1,

        [Description("Clean Credit Customer")]
        CC = 2,

        [Description("Cash Customer")]
        CS = 3
    }
}
