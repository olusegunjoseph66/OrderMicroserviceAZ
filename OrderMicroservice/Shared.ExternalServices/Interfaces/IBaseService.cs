

using Shared.ExternalServices.DTOs;

namespace Shared.ExternalServices.Interfaces
{
    public interface IBaseService : IDisposable
    {
        ResponseDto response { get; set; }
        Task<T> SendAsync<T>(ApiRequest apiRequest);
    }
}
