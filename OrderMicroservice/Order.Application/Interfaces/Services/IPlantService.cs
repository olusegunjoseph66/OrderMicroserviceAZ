using Order.Application.DTOs.APIDataFormatters;
using Order.Application.ViewModels.QueryFilters;
using Order.Application.ViewModels.Responses;
using Shared.Utilities.DTO.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Interfaces.Services
{
    public interface IPlantService
    {
        Task<ApiResponse> UpdatePlant();
        Task<ApiResponse> GetPlants(PlantQueryFilter filter, CancellationToken cancellationToken);
        Task<ApiResponse> GetTruckSizes(TruckSizesQueryFilter filter, CancellationToken cancellationToken);
        Task<ApiResponse> GetTruckSizesV2(TruckSizesQueryFilterV2 filter, CancellationToken cancellationToken);
        Task<ApiResponse> GetUnitMeasure(CancellationToken cancellationToken);
        Task<ApiResponse> GetDelivaryMethods(CancellationToken cancellationToken);

    }
}
