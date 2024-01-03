using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Interfaces.Messaging
{
    public interface IAzureServiceBusConsumer
    {
        Task StartOrderMsg();
        Task StopOrderMsg();
        Task StartAccountMsg();
        Task StopAccountMsg();
        Task StartProductMsg();
        Task StopProductMsg();
    }
}
