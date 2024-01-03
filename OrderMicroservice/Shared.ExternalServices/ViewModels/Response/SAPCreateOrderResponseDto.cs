using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vm.ViewModels.Responses
{

    public class SAPResponseDtoForAll
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }




public class CreateSapOrderResponseObj
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public Data22354 data { get; set; }
    }

    public class Data22354
    {
        public Saporder288787 sapOrder { get; set; }
    }

    public class SapOrderCreateResponse
    {
        public bool ActionFailed { get; set; } = false;
        public Saporder288787 SapOrder { get; set; }
    }

    public class Saporder288787
    {
        public long Id { get; set; }
        public string dateCreated { get; set; }
        public string netValue { get; set; }
        public float vat { get; set; }
        public string reference { get; set; }
        public int lineItemId { get; set; }
        public float lineItemPricePerUnit { get; set; }
        public float lineItemNetValue { get; set; }
    }


    public class Ordertype111
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Status222
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Orderitem1214
    {
        public int Id { get; set; }
        public int orderId { get; set; }
        public Product product { get; set; }
        public string orderQuantity { get; set; }
        public Salesunitofmeasure salesUnitOfMeasure { get; set; }
        public string pricePerUnit { get; set; }
        public string netValue { get; set; }
    }

    public class Product
    {
        public long productId { get; set; }
        public long name { get; set; }
        public string productType { get; set; }
    }

    public class Salesunitofmeasure
    {
        public string code { get; set; }
        public string name { get; set; }
    }


}
