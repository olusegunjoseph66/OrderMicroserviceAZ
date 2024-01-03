using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.ViewModels.Request
{
    public class ProofRequestDto
    {
        public int distributorSapAccountId { get; set; }
        public string atcNumber { get; set; }
    }
}
