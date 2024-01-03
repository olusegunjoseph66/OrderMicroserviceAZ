using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Enums
{
    public enum OrderStatus
    {
        [Description("New")]
        New = 1,

        [Description("Saved")]
        Saved = 2,

        [Description("Cancelled")]
        Cancelled = 3,

        [Description("Processing")]
        Processing = 4,

        [Description("Processed")]
        Processed = 5,

        [Description("Failed")]
        Failed = 6,

        [Description("Submitted")]
        Submitted = 7,

        [Description("Pending Otp Validation")]
        PendingOtp = 8,

        [Description("Processing Submission")]
        ProcessingSubmission = 9,

        [Description("Otp Validated")]
        Validated = 10,

        [Description("InValidated")]
        InValidated = 11,

        [Description("Goods Delivered")]
        GoodsDelivered = 14,

        [Description("Goods Collected")]
        GoodsCollected = 15,

    }
}
