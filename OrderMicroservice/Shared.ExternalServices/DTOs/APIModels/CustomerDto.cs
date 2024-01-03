using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.DTOs.APIModels
{
    public class CustomerDto
    {
        public string Id { get; set; }
        public string DistributorName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public Status status { get; set; }
    }
    public class Status
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
