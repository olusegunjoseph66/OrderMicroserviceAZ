using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Enums
{
    public enum OtpStatusEnum
    {
        [Description("New")]
        New = 1,

        [Description("Validated")]
        Validated = 2,

        [Description("Invalidated")]
        Invalidated = 3
    }
}
