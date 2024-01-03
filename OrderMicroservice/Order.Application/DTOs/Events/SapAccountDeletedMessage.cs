using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class SapAccountDeletedMessage 
    {
        public string DistributorSapNumber { get; set; }
        public int SapAccountId { get; set; }
        public int UserId { get; set; }
        public DateTime DateDeleted { get; set; }
        public DateTime DateCreated { get; set; }

    }
}
