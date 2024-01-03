using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class DmsAlertErrorMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Source { get; set; }
        public string Message { get; set; }
        public DateTime DateReported { get; set; } = DateTime.UtcNow;
    }
}
