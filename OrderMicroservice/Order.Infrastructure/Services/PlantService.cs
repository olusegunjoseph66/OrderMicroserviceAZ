using Account.Application.Constants;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Order.Application.Constants;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Filters;
using Order.Application.Exceptions;
using Order.Application.Interfaces.Services;
using Order.Application.ViewModels.QueryFilters;
using Order.Application.ViewModels.Responses;
using Order.Infrastructure.QueryObjects;
using Shared.Data.Extensions;
using Shared.Data.Models;
using Shared.Data.Repository;
using Shared.ExternalServices.Interfaces;
using Shared.Utilities.DTO.Pagination;
using Shared.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Infrastructure.Services
{
    public class PlantService : BaseService, IPlantService
    {
        private readonly IAsyncRepository<Plant> _plantDb;
        private readonly IAsyncRepository<DeliveryMethod> _delivaryRepository;
        private readonly IAsyncRepository<TruckSize> _truckSizeRepository;
        private readonly IAsyncRepository<UnitOfMeasure> _unitOfMeasureRepository;
        private readonly IAsyncRepository<DmsOrderGroup> _orderGroup;
        private readonly IAsyncRepository<ShoppingCart> _cart;
        private readonly IAsyncRepository<TruckSizeDeliveryMethodMapping> _delivaryMethodRepository;

        private readonly ILogger<PlantService> _plantLogger;
        private readonly ISapService _sapService;
        public readonly IQueueMessagingService _messageBus;
        public PlantService(IAsyncRepository<Plant> plantDb, IAsyncRepository<TruckSize> truckSizeRepository,
            IAsyncRepository<TruckSizeDeliveryMethodMapping> delivaryMethodRepository,
            IAsyncRepository<DeliveryMethod> delivaryRepository, IAsyncRepository<UnitOfMeasure> unitOfMeasureRepository,
            ILogger<PlantService> plantLogger, IAsyncRepository<DmsOrderGroup> _orderGroup,
            ISapService sapService, IQueueMessagingService messageBus,
            IAuthenticatedUserService authenticatedUserService, IAsyncRepository<ShoppingCart> cart) : base(authenticatedUserService)
        {
            _plantDb = plantDb;
            _plantLogger = plantLogger;
            _sapService = sapService;
            _messageBus = messageBus;
            _delivaryRepository = delivaryRepository;
            _truckSizeRepository = truckSizeRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _delivaryMethodRepository = delivaryMethodRepository;
            this._orderGroup = _orderGroup;
            _cart = cart;
        }

        public async Task<ApiResponse> UpdatePlant()
        {
            _plantLogger.LogInformation($"{"About to Update DMS Plant"}{" | "}{DateTime.UtcNow}");

            var companyDetails = await _sapService.GetCompanies();
            _plantLogger.LogInformation($"{"DMS Company RDATA Response:-"}{" | "}{JsonConvert.SerializeObject(companyDetails)}");

            foreach (var companyItem in companyDetails)
            {
                var plants = await _sapService.GetPlant(companyItem.Code,/* companyItem.CountryCode*/ "NG");
                _plantLogger.LogInformation($"{"DMS Plant SAP Response:-"}{" | "}{JsonConvert.SerializeObject(plants)}");

                var plantsInDb = await _plantDb.Table.AsNoTracking().Where(c => c.CompanyCode == companyItem.Code).ToListAsync();
                List<Plant> plantsToUpdate = new();
                foreach (var plantItem in plants)
                {
                    try
                    {
                        var plantInDb = plantsInDb.FirstOrDefault(c => c.Code == plantItem.Code);
                        if (plantInDb == null)
                        {
                            plantsToUpdate.Add(new Plant
                            {
                                CompanyCode = plantItem.CompanyCode,
                                CountryCode = plantItem.CountryCode,
                                Address = plantItem.Address,
                                DateRefreshed = DateTime.UtcNow,
                                Code = plantItem.Code,
                                Name = plantItem.Name,
                                PlantTypeCode = (plantItem.Name.ToLower().Contains("depot") || plantItem.Name.ToLower().EndsWith("dp")) ? "Depot" : "Plant",
                            });
                        }
                        else
                        {
                            plantInDb.CompanyCode = plantItem.CompanyCode;
                            plantInDb.CountryCode = plantItem.CountryCode;
                            plantInDb.Address = plantItem.Address;
                            plantInDb.DateRefreshed = DateTime.UtcNow;
                            plantInDb.Name = plantItem.Name;
                            plantInDb.PlantTypeCode = (plantItem.Name.ToLower().Contains("depot") || plantItem.Name.ToLower().EndsWith("dp")) ? "Depot" : "Plant";
                            plantsToUpdate.Add(plantInDb);
                        }
                    }
                    catch (Exception e)
                    {
                        _plantLogger.LogError($"Cannot Update Plant | Date : {DateTime.UtcNow} Plant: {JsonConvert.SerializeObject(plantItem)} error: {e.Message}");
                    }
                }
                _plantDb.Table.UpdateRange(plantsToUpdate);
                await _plantDb.CommitAsync(default);
            }
            return ResponseHandler.SuccessResponse("Updated Successfully");
        }
        public async Task<ApiResponse> GetPlants(PlantQueryFilter filter, CancellationToken cancellationToken)
        {
            GetUserId();

            PlantFilterDto plantFilter = new()
            {
                CompanyCode = filter.CompanyCode,
                CountryCode = filter.CountryCode
            };

            var expression = new PlantQueryObject(plantFilter).Expression;

            var query = _plantDb.Table.AsNoTrackingWithIdentityResolution()
                    .OrderByWhere(expression, null);

            //var totalCount = await query.CountAsync(cancellationToken);

            //query = query.Select(ux => new Plant
            //{
            //    Id = ux.Id,
            //    Address = ux.Address,
            //    Code = ux.Code,
            //    CompanyCode = ux.CompanyCode,
            //    CountryCode = ux.CountryCode,
            //    Name = ux.Name,
            //    PlantTypeCode = ux.PlantTypeCode,
            //    DateRefreshed = ux.DateRefreshed

            //}).Paginate(pageFilter.PageNumber, pageFilter.PageSize);

            var plants = await query.ToListAsync(cancellationToken);
            if (!plants.Any())
                return ResponseHandler.SuccessResponse("No Record Found");

            //var totalPages = NumberManipulator.PageCountConverter(totalCount, pageFilter.PageSize);
            //var response = new PaginatedList<PlantResponse>(ProcessQuery(plants), new PaginationMetaData(filter.PageIndex, filter.PageSize, totalPages, totalCount));

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_PLANT_LIST, new { plants = plants.OrderBy(x => x.Name) });
        }

        public async Task<ApiResponse> GetTruckSizes(TruckSizesQueryFilter filter, CancellationToken cancellationToken)
        {
            GetUserId();

            TruckSizesFilterDto plantFilter = new()
            {
                DeliveryMethodCode = filter.DeliveryMethodCode,
                PlantTypeCode = filter.PlantTypeCode
            };
            var expression = new TruckSizesQueryObject(plantFilter).Expression; ;
            var query = _delivaryMethodRepository.Table.AsNoTrackingWithIdentityResolution()
                    .OrderByWhere(expression, null);
            var totalCount = await query.CountAsync(cancellationToken);

            query = query.Select(ux => new TruckSizeDeliveryMethodMapping
            {
                Id = ux.Id,
                DeliveryMethodCode = ux.DeliveryMethodCode,
                TruckSizeCode = ux.TruckSizeCode,
            });

            var truckSizes = await query.ToListAsync(cancellationToken);
            var response = ProcessQuery(truckSizes);
            if (response == null || response?.Count == 0)
                return ResponseHandler.SuccessResponse("No TruckSize Availabe");


            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_PLANT_LIST, new { trucksizes = response });
        }

        public async Task<ApiResponse> GetTruckSizesV2(TruckSizesQueryFilterV2 filter, CancellationToken cancellationToken)
        {
            GetUserId();

            var dmsOrderGroup = await _orderGroup.Table.Include(x => x.ShoppingCart).FirstOrDefaultAsync(x => x.Id == filter.DmsOrderGroupId);
            var sCart = await _cart.Table.Include(x => x.DistributorSapAccount).FirstOrDefaultAsync(x => x.Id == dmsOrderGroup.ShoppingCartId);
            var distributorSapAccount = sCart.DistributorSapAccount;
            if (distributorSapAccount == null || distributorSapAccount.UserId != LoggedInUserId)
            {
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);
                //Error-O-01
            }

            var companyCode = distributorSapAccount?.CompanyCode;
            //List<TruckSizeDeliveryMethodMapping> truckSizes = null;
            var plant = await _plantDb.Table.Where(x => x.Code == dmsOrderGroup.ShoppingCart.PlantCode).FirstOrDefaultAsync();
            if (companyCode == "1000")
            {
                //TruckSizesFilterDto plantFilter = new()
                //{
                //    //DeliveryMethodCode = filter.DeliveryMethodCode,
                //    PlantTypeCode = plant.PlantTypeCode
                //};
                //var expression = new TruckSizesQueryObject(plantFilter).Expression; ;
                //var query = _delivaryMethodRepository.Table.AsNoTrackingWithIdentityResolution()
                //        .OrderByWhere(expression, null);

                //query = query.Select(ux => new TruckSizeDeliveryMethodMapping
                //{
                //    Id = ux.Id,
                //    DeliveryMethodCode = ux.DeliveryMethodCode,
                //    TruckSizeCode = ux.TruckSizeCode,
                //});

                //truckSizes = await query.ToListAsync(cancellationToken);
                //var responses = ProcessQuery(truckSizes);
                //if (responses == null || responses?.Count == 0)
                //    return ResponseHandler.SuccessResponse("No TruckSize Availabe");
                var responses = await (from trucks in _truckSizeRepository.Table select new { code = trucks.Code, name = trucks.Name }).ToListAsync();
                if (plant.PlantTypeCode == "Plant")
                {
                    responses = responses.Where(x => x.code != "300").ToList();
                }

                return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_PLANT_LIST, new { trucksizes = responses });
            }
            else
            {
                var responses = await (from trucks in _truckSizeRepository.Table select new { code = trucks.Code, name = trucks.Name }).ToListAsync();
                if (responses == null || responses?.Count == 0)
                    return ResponseHandler.SuccessResponse("No TruckSize Availabe");


                return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_PLANT_LIST, new { trucksizes = responses });
            }

        }

        public async Task<ApiResponse> GetDelivaryMethods(CancellationToken cancellationToken)
        {
            var delivarys = await _delivaryRepository.Table.Select(x => new DeliveryMethod
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
            }).ToListAsync(cancellationToken);


            var delivaries = delivarys.Select(x => new { name = x.Name, code = x.Code }).ToList();
            if (delivaries == null || delivaries?.Count == 0)
                return ResponseHandler.SuccessResponse("No Record Found");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_DELIVARYMETHOD, new { deliveryMethods = delivaries });
        }

        public async Task<ApiResponse> GetUnitMeasure(CancellationToken cancellationToken)
        {
            var measurs = await _unitOfMeasureRepository.Table.Select(x => new UnitOfMeasure
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            }).ToListAsync(cancellationToken);


            var unitofmeaasures = measurs.Select(x => new { x.Name, code = x.Code }).ToList();
            if (unitofmeaasures == null || unitofmeaasures?.Count == 0)
                return ResponseHandler.SuccessResponse("No Record Found");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_UNITOFMEASURE, new { unitOfMeasures = unitofmeaasures });
        }

        private static IReadOnlyList<PlantResponse> ProcessQuery(IReadOnlyList<Plant> plants)
        {
            return plants.Select(p =>
            {
                var plantStatus = new PlantTypeResponse
                {
                    //Code = p.Code,
                    Code = p.PlantTypeCode,
                    Name = p.PlantTypeCode
                };

                var item = new PlantResponse(p.Id, p.CompanyCode, p.CountryCode, p.Name, plantStatus, p.Code);
                return item;
            }).ToList();
        }

        private IReadOnlyList<TruckSizesResponces> ProcessQuery(IReadOnlyList<TruckSizeDeliveryMethodMapping> plants)
        {

            return plants.Select(p =>
            {
                var tru = _truckSizeRepository.Table.FirstOrDefault(c => c.Code == p.TruckSizeCode);
                var item = new TruckSizesResponces(tru.Code, tru.Name);
                return item;
            }).ToList();
        }
    }
}
