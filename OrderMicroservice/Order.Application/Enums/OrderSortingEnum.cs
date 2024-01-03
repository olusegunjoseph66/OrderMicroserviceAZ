using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Enums
{
    public enum OrderSortingEnum
    {
        [Description("date_asc")]
        DateAscending = 1,

        [Description("date_desc")]
        DateDescending = 2,

        [Description("value_desc")]
        ValueDescending = 3,

        [Description("value_asc")]
        ValueAscending = 4,
    }
}
