using Order.Application.DTOs.APIDataFormatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Interfaces.Services
{
    public interface IDeadLetterService
    {
        Task<ApiResponse> ProcessAccountDeadLetterMessages(CancellationToken cancellationToken = default);
    }
}
