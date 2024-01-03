using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.ViewModels.Response
{
    public class WalletResponse
    {
        public int Id { get; set; }
        public decimal AvailableBalance { get; set; }
    }

    public class SAPWalletResponse1
    {
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public Sappy data { get; set; }
    }

    public class Sappy
    {
        public Sapwallet sapWallet { get; set; }
    }

    public class Sapwallet
    {
        public decimal AvailableBalance { get; set; }
        //public Transactionsummary[] transactionSummary { get; set; }
    }

    public class Transactionsummary
    {
        public string description { get; set; }
        public int amount { get; set; }
    }

}
