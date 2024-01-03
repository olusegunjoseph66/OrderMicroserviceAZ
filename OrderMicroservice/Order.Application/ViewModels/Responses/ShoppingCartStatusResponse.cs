using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class ShoppingCartStatusResponse
    {
        public ShoppingCartStatusResponse(string code, string name)
        {
            Code = code;
            Name = name;
        }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
