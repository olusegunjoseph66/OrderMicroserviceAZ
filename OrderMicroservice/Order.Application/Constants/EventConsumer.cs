using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Constants
{
    public class EventConsumer
    {
        public const string ORDER_DMS_UPDATED_SUBSCRIPTION = "Orders.DmsOrder.Updated";
        public const string ORDER_OTP_GENERATED_SUBSCRIPTION = "Orders.Otp.Generated";
        public const string ORDER_PLANT_REFRESHED_SUBSCRIPTION = "Orders.Plant.Refreshed";
    }
}
