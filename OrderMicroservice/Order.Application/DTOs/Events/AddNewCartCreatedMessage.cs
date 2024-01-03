using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class AddNewCartCreatedMessage : IntegrationBaseMessage
    {
        public int UserId { get; set; }
        public int ShoppingCartId { get; set; }
        public int CreatedByUserId { get; set; }
        public StatusDto ShoppingCartStatus { get; set; }       

    }

    public class StatusDto
    {
        public string code { get; set; }
        public string name { get; set; }
    }
}
