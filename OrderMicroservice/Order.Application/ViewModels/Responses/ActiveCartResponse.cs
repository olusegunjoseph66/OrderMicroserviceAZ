using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{

    public class ActiveCartResponse
    {
        public int ShoppingCartId { get; set; }
        public int UserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DateCreated { get; set; }
        public int ModifiedByUserId { get; set; }
        public ShoppingCartStatusResponse  ShoppingCartStatus { get; set; }
        public DateTime? DateModified { get; set; }
        public List<ShoppingCartItemsResponse>  ShoppingCartItems  { get; set; }

     
    }
    public class ActiveCartResponseVM
    {
        public ActiveCartResponse2 shoppingCart { get; set; }


    }


    public class ActiveCartResponse2
    {
        public int ShoppingCartId { get; set; }
        public DateTime? DateCreated { get; set; }
        public int CreatedByUserId { get; set; }
        public int ModifiedByUserId { get; set; }
        public int UserId { get; set; }
        public ShoppingCartStatusResponse ShoppingCartStatus { get; set; }
        public DateTime? DateModified { get; set; }
        //public distributorSapAccountResponse distributorSapAccount { get; set; }
        //public string plantCode { get; set; }
        //public string deliveryMethodCode { get; set; }
        public decimal totalEstimatedValue { get; set; } = 0;
        //public decimal totalFreightCharges { get; set; } = 0;
        //public decimal totalVat { get; set; } = 0;
        public List<ShoppingCartItemsResponse2> ShoppingCartItems { get; set; }


    }

    public class ActiveCartResponseVM2
    {
        public ActiveCartResponse2 shoppingCart { get; set; }

    }

    public class distributorSapAccountResponse
    {
        public int distributorSapAccountId { get; set; }
        public string distributorSapNumber { get; set; }
        public string distributorName { get; set; }
        public string companyCode { get; set; }
        public string countryCode { get; set; }
    }
}

