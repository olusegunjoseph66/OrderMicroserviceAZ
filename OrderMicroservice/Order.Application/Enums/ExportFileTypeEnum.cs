using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Enums
{
    public enum ExportFileTypeEnum
    {
        [Description("pdf")]
        PdfFile = 1,

        [Description("xls")]
        Xls = 2,

        [Description("csv")]
        Csv = 3,

    }
}
