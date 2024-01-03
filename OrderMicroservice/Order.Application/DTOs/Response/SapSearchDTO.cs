using Order.Application.ViewModels.Requests;
using Shared.ExternalServices.DTOs.APIModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Response
{
    public class SapSearchDTO
    {

        public string Id { get; set; }
        public string ParentId { get; set; }
        public DateTime DateCreated { get; set; }
        public string DistributorNumber { get; set; }
        public string NumberOfItem { get; set; }
        public string NetValue { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryDate { get; set; }
        public Status Status { get; set; }
        public Status OrderType { get; set; }
        public Status DeliveryBlock { get; set; }

    }
    public class SAPSearchList
    {
        public SapSearchDTO sapOrders { get; set; }
    }
}
