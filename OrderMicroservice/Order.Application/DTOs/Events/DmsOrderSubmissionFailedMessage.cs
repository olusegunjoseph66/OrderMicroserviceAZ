using Order.Application.ViewModels.Responses;
using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class DmsOrderSubmissionFailedMessage : IntegrationBaseMessage
    {

        public int DmsOrderId { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateSubmittedtoDMS { get; set; }
        public int UserId { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public FailOrderDistributorSapAccount DistributorSapAccount { get; set; }
        public decimal EstimatedNetValue { get; set; }
        public StatusRep OrderStatus { get; set; }
        public StatusRep OrderType { get; set; }
        public string TruckSizeCode { get; set; }
        public string DeliveryMethodCode { get; set; }
        public FailOrdersPlant Plant { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryStateCode { get; set; }
        public string DeliveryCountryCode { get; set; }
        public int NumberOfSubmissionAttempts { get; set; }
        public List<FailSubmissionOrderItemsResponse> DmsOrderItems { get; set; }
    }

    public class FailOrderDistributorSapAccount
    {
        public FailOrderDistributorSapAccount(int distributorSapAccountId, string distributorSapNumber)
        {
            DistributorSapAccountId = distributorSapAccountId;
            DistributorSapNumber = distributorSapNumber;
        }

        public int DistributorSapAccountId { get; set; }
        public string DistributorSapNumber { get; set; }
    }

    public class FailOrdersPlant
    {
        public FailOrdersPlant(int plantId, string code, string name)
        {
            PlantId = plantId;
            Code = code;
            Name = name;
        }

        public int PlantId { get; set;}
        public string Code { get; set;}
        public string Name { get; set;}
    }
}
