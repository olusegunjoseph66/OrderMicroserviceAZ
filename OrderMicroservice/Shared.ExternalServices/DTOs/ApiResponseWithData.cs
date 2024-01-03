using Shared.ExternalServices.ViewModels.Response;

namespace Shared.ExternalServices.DTOs
{
    public  class ApiResponse
    {

        public CountryVms data { get; set; }
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
    public class ApiResponseState
    {

        public StateVm data { get; set; }
        public string statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
    public class StateVm
    {
        public List<CountryResponse> state { get; set; }
    }
    public partial class ResponseDto
    {
        public object Data { get; set; } = null;
        public string Status { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
    }
}
