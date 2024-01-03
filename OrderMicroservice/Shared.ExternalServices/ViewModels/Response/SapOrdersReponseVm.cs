using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vm.ViewModels.Responses
{ 
    public class SapOrdersReponseVm
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public SAPOrderList data { get; set; }
    }

    public class SAPOrderList
    {
        public Saporder234567[] sapOrders { get; set; }
    }

    public class Saporder234567
    {
        public string Id { get; set; }
        public string parentId { get; set; }
        public string dateCreated { get; set; }
        public double netValue { get; set; }
        public Status Status { get; set; }
        public Ordertype orderType { get; set; }
        public string distributorNumber { get; set; }
        public int numberOfItems { get; set; }
    }

    public class Status
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Ordertype
    {
        public string code { get; set; }
        public string name { get; set; }
    }

}
