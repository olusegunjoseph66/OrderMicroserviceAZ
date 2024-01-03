using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.Interfaces.Services;
using Order.Application.ViewModels.QueryFilters;
using Order.Application.ViewModels.Responses;
using Shared.Utilities.DTO.Pagination;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlantController : BaseController
    {
        private readonly IPlantService _plantService;
        public PlantController(IPlantService plantService)
        {
            _plantService = plantService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginatedList<PlantResponse>>))]
        public async Task<IActionResult> GetPlants([FromQuery] PlantQueryFilter filter, CancellationToken cancellationToken = default)
        {
            return Response(await _plantService.GetPlants(filter, cancellationToken));
        }
        
        [HttpGet("trucksizes")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TruckSizesResponcesVm>))]
        public async Task<IActionResult> GetTruckSizes([FromQuery] TruckSizesQueryFilter filter, CancellationToken cancellationToken = default)
        {
            return Response(await _plantService.GetTruckSizes(filter, cancellationToken));
        }
        [HttpGet("trucksizes/V2")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TruckSizesResponcesVm>))]
        public async Task<IActionResult> GetTruckSizesV2([FromQuery] TruckSizesQueryFilterV2 filter, CancellationToken cancellationToken = default)
        {
            return Response(await _plantService.GetTruckSizesV2(filter, cancellationToken));
        }

        [HttpGet("deliverymethod")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<DeliveryMethodResponse>))]
        public async Task<IActionResult> GetDelivaryMethod(CancellationToken cancellationToken = default)
        {
            return Response(await _plantService.GetDelivaryMethods(cancellationToken));
        }

        [HttpGet("unitOfMeasures")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UnitofMeasureResponse>))]
        public async Task<IActionResult> GetUnitOfMeasures(CancellationToken cancellationToken = default)
        {
            //User.GetSessionDetails
            return Response(await _plantService.GetUnitMeasure(cancellationToken));
        }
    }
}
