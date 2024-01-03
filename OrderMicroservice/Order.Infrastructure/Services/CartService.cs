using Account.Application.Constants;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Order.Application.Constants;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Events;
using Order.Application.DTOs.Features;
using Order.Application.Enums;
using Order.Application.Exceptions;
using Order.Application.Interfaces.Services;
using Order.Application.ViewModels.Requests;
using Order.Application.ViewModels.Responses;
using Shared.Data.Models;
using Shared.Data.Repository;
using Shared.ExternalServices.Interfaces;
using Shared.ExternalServices.ViewModels.Request;
using Shared.Utilities.Helpers;

namespace Order.Infrastructure.Services
{
    public class CartService : BaseService, ICartService
    {
        private readonly IAsyncRepository<Product> _productRepository;
        private readonly IAsyncRepository<DistributorSapAccount> _distributorSapAccountRepository;
        private readonly IAsyncRepository<ShoppingCart> _shoppingCartRepository;
        private readonly IAsyncRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IAsyncRepository<ShoppingCartStatus> _shoppingCartStatusRepository;
        public readonly IQueueMessagingService _messageBus;
        private readonly IAsyncRepository<Plant> _plant;
        private readonly ICachingService _cache;
        private readonly ISapService _sapService;
        private readonly ILogger<CartService> _cartLogger;
        public CartService(IAsyncRepository<Product> productRepository,
            IAsyncRepository<DistributorSapAccount> distributorSapAccountRepository,
            IAsyncRepository<ShoppingCart> shoppingCartRepository,
            ICachingService cache, IAsyncRepository<Plant> _plant,
            ISapService sapService,
            IAsyncRepository<ShoppingCartItem> shoppingCartItemRepository,
            IAsyncRepository<ShoppingCartStatus> shoppingCartStatusRepository,
            IQueueMessagingService messageBus,
            IAuthenticatedUserService authenticatedUserService,
            ILogger<CartService> cartLogger) : base(authenticatedUserService)
        {
            _productRepository = productRepository;
            _distributorSapAccountRepository = distributorSapAccountRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _cache = cache;
            _sapService = sapService;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _shoppingCartStatusRepository = shoppingCartStatusRepository;
            _messageBus = messageBus;
            _cartLogger = cartLogger;
            this._plant = _plant;
        }

        public async Task<ApiResponse> AddProduct(AddProductToCartRequest request, CancellationToken cancellationToken)
        {
            GetUserId();

            var productDetail = _productRepository.Table.FirstOrDefault(p => p.Id == request.ProductId);

            if (productDetail == null)
                throw new NotFoundException(ErrorCodes.PRODUCT_NOTFOUND.Key, ErrorCodes.PRODUCT_NOTFOUND.Value);
            //Error-O-03
            var sapDistributorAccount = await _distributorSapAccountRepository.Table.FirstOrDefaultAsync(sap => sap.DistributorSapAccountId == request.DistributorSapAccountId);

            if (sapDistributorAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);
            //Error-O-01 

            if (sapDistributorAccount.AccountType != AccountTypeEnum.CS.ToDescription())
                throw new NotFoundException(ErrorCodes.INVALID_ACCOUNT_TO_PLACE_ORDER.Key, ErrorCodes.INVALID_ACCOUNT_TO_PLACE_ORDER.Value);
            //Error-O-04

            if (sapDistributorAccount.CompanyCode != productDetail.CompanyCode)
                throw new NotFoundException(ErrorCodes.INVALID_COMPANYCODE_TO_PLACE_ORDER.Key, ErrorCodes.INVALID_COMPANYCODE_TO_PLACE_ORDER.Value);
            //Error-O-05


            var shoppingCart = await _shoppingCartRepository.Table.Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active)
                                               .FirstOrDefaultAsync();


            if (shoppingCart == null)
            {
                ShoppingCart addNewShoppingCart = new()
                {
                    UserId = LoggedInUserId,
                    CreatedByUserId = LoggedInUserId,
                    DateCreated = DateTime.UtcNow,
                    ShoppingCartStatusId = (int)ShoppingCartStatusEnum.Active,
                };
                await _shoppingCartRepository.AddAsync(addNewShoppingCart, cancellationToken);

                await _shoppingCartRepository.CommitAsync(cancellationToken);



                //Azure ServiceBus Orders.ShoppingCart.Created
                AddNewCartCreatedMessage newCartCreatedMessage = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = addNewShoppingCart.UserId,
                    CreatedByUserId = addNewShoppingCart.CreatedByUserId,
                    DateCreated = DateTime.UtcNow,
                    ShoppingCartId = addNewShoppingCart.Id,
                    ShoppingCartStatus = new StatusDto()
                    {
                        code = _shoppingCartStatusRepository.Table.Where(u => u.Id == addNewShoppingCart.ShoppingCartStatusId).FirstOrDefault()?.Code,
                        name = _shoppingCartStatusRepository.Table.Where(u => u.Id == addNewShoppingCart.ShoppingCartStatusId).FirstOrDefault()?.Name
                    },
                };

                await _messageBus.PublishTopicMessage(newCartCreatedMessage, EventMessages.ORDER_SHOPPINGCART_CREATED);

                var shoppingCartItemsi = await _shoppingCartItemRepository.Table.Where(sci => sci.ShoppingCartId == addNewShoppingCart.Id
                                                && sci.ProductId == request.ProductId
                                                && sci.UnitOfMeasureCode == request.UnitOfMeasureCode).FirstOrDefaultAsync(cancellationToken);
                if (shoppingCartItemsi == null)
                {
                    ShoppingCartItem addNewiCart = new()
                    {
                        DistributorSapAccountId = request.DistributorSapAccountId,
                        CreatedByUserId = LoggedInUserId,
                        ShoppingCartId = addNewShoppingCart.Id,
                        DateCreated = DateTime.UtcNow,
                        Quantity = request.Quantity,
                        ProductId = (short)request.ProductId,
                        ChannelCode = request.ChannelCode,
                        UnitOfMeasureCode = request.UnitOfMeasureCode
                    };
                    await _shoppingCartItemRepository.AddAsync(addNewiCart, cancellationToken);

                    await _shoppingCartItemRepository.CommitAsync(cancellationToken);
                }
                else
                {
                    var OldShoppingCartItems = shoppingCartItemsi;

                    shoppingCartItemsi.DistributorSapAccountId = request.DistributorSapAccountId;
                    shoppingCartItemsi.ModifiedByUserId = LoggedInUserId;
                    shoppingCartItemsi.DateModified = DateTime.UtcNow;
                    shoppingCartItemsi.ChannelCode = request.ChannelCode;
                    shoppingCartItemsi.Quantity = shoppingCartItemsi.Quantity + request.Quantity;

                    await _shoppingCartItemRepository.UpdateAsync(shoppingCartItemsi);

                    _shoppingCartItemRepository.Commit();

                    //Azure ServiceBus Orders.ShoppingCart.Updated
                    var msgItems = new List<ShoppingCartItem11>();
                    foreach (var item in shoppingCart.ShoppingCartItems)
                    {
                        var product = _productRepository.Table.Where(x => x.Id.Equals(item.ProductId)).FirstOrDefault();
                        msgItems.Add(new ShoppingCartItem11
                        {
                            dateCreated = item.DateCreated,
                            Quantity = item.Quantity,
                            ShoppingCartItemId = item.Id,
                            UnitOfMeasureCode = item.UnitOfMeasureCode,
                            Product = product == null ? null : new Product11
                            {
                                Name = product.Name,
                                ProductId = item.Product.Id,
                                Price = product.Price,
                            }
                        });
                    }
                    NewCartUpdatedMessage newCartUpdatedMessage = new()
                    {
                        UserId = LoggedInUserId,
                        DateModified = shoppingCart.DateModified,
                        ModifiedByUserId = LoggedInUserId,
                        ShoppingCartId = OldShoppingCartItems.ShoppingCartId,
                        newShoppingCartStatus = new StatusDto
                        {
                            name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCart.Id).ShoppingCartStatusId)).FirstOrDefault().Name,
                            code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCart.Id).ShoppingCartStatusId)).FirstOrDefault().Code,

                        },
                        OldShoppingCartStatus = new StatusDto
                        {
                            name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                            code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,

                        },
                        DateCreated = shoppingCart.DateCreated,
                        Id = Guid.NewGuid(),
                        ShoppingCartItems = msgItems

                    };

                    await _messageBus.PublishTopicMessage(newCartUpdatedMessage, EventMessages.ORDER_SHOPPINGCART_UPDATED);

                }
            }
            else
            {
                var shoppingCartExist = await _shoppingCartRepository.Table.Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active)
                                              .FirstOrDefaultAsync();

                if (shoppingCartExist != null)
                {

                    var cartItems = await _shoppingCartItemRepository.Table.Where(sci => sci.ShoppingCartId == shoppingCartExist.Id).FirstOrDefaultAsync(cancellationToken);

                    //if (cartItems.ProductId != request.ProductId)
                    //{
                    //    return ResponseHandler.FailureResponse("E05", "You cannot Add multiple line Items to Cart At this time");
                    //}

                    var shoppingCartItems = await _shoppingCartItemRepository.Table.Where(sci => sci.ShoppingCartId == shoppingCartExist.Id
                                                    && sci.ProductId == request.ProductId
                                                    && sci.UnitOfMeasureCode == request.UnitOfMeasureCode).FirstOrDefaultAsync(cancellationToken);

                    if (shoppingCartItems == null)
                    {
                        ShoppingCartItem addNewiiCart = new()
                        {
                            DistributorSapAccountId = request.DistributorSapAccountId,
                            CreatedByUserId = LoggedInUserId,
                            ShoppingCartId = shoppingCartExist.Id,
                            DateCreated = DateTime.UtcNow,
                            Quantity = request.Quantity,
                            ProductId = (short)request.ProductId,
                            ChannelCode = request.ChannelCode,
                            UnitOfMeasureCode = request.UnitOfMeasureCode
                        };
                        await _shoppingCartItemRepository.AddAsync(addNewiiCart, cancellationToken);

                        await _shoppingCartItemRepository.CommitAsync(cancellationToken);
                    }
                    else
                    {
                        var OldShoppingCartItems = shoppingCartItems;


                        shoppingCartItems.DistributorSapAccountId = request.DistributorSapAccountId;
                        shoppingCartItems.ModifiedByUserId = LoggedInUserId;
                        shoppingCartItems.DateModified = DateTime.UtcNow;
                        shoppingCartItems.ChannelCode = request.ChannelCode;
                        shoppingCartItems.Quantity = shoppingCartItems.Quantity + request.Quantity;

                        await _shoppingCartItemRepository.UpdateAsync(shoppingCartItems);

                        _shoppingCartItemRepository.Commit();

                        var shoppingCartUpdate = await _shoppingCartRepository.Table.Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active)
                                                       .FirstOrDefaultAsync(cancellationToken);

                        shoppingCart.DateModified = DateTime.UtcNow;
                        await _shoppingCartRepository.UpdateAsync(shoppingCartUpdate);
                        _shoppingCartRepository.Commit();

                        //Azure ServiceBus Orders.ShoppingCart.Updated
                        var msgItems = new List<ShoppingCartItem11>();
                        foreach (var item in shoppingCart.ShoppingCartItems)
                        {
                            msgItems.Add(new ShoppingCartItem11
                            {
                                dateCreated = item.DateCreated,
                                Quantity = item.Quantity,
                                ShoppingCartItemId = item.Id,
                                UnitOfMeasureCode = item.UnitOfMeasureCode,
                                Product = new Product11
                                {
                                    Name = item.Product.Name,
                                    ProductId = item.Product.Id,
                                    Price = item.Product.Price,
                                }
                            });
                        }
                        NewCartUpdatedMessage newCartUpdatedMessage = new()
                        {
                            UserId = LoggedInUserId,
                            DateModified = shoppingCartItems.DateModified,
                            ModifiedByUserId = LoggedInUserId,
                            ShoppingCartId = OldShoppingCartItems.ShoppingCartId,
                            newShoppingCartStatus = new StatusDto
                            {
                                name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                                code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,

                            },
                            OldShoppingCartStatus = new StatusDto
                            {
                                name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                                code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,

                            },
                            DateCreated = shoppingCart.DateCreated,
                            Id = Guid.NewGuid(),
                            ShoppingCartItems = msgItems

                        };

                        await _messageBus.PublishTopicMessage(newCartUpdatedMessage, EventMessages.ORDER_SHOPPINGCART_UPDATED);

                    }
                }

            }


            return ResponseHandler.SuccessResponse(SuccessMessages.NEW_PRODUCT_ADDED_SUCCESSFULLY_TO_CART);
        }
        public async Task<ApiResponse> AddProductV2(AddProductToCartRequestV2 request, CancellationToken cancellationToken)
        {
            GetUserId();

            var productDetail = _productRepository.Table.FirstOrDefault(p => p.Id == request.ProductId);

            if (productDetail == null)
                throw new NotFoundException(ErrorCodes.PRODUCT_NOTFOUND.Value, ErrorCodes.PRODUCT_NOTFOUND.Key);
            //Error-O-03
            var sapDistributorAccount = await _distributorSapAccountRepository.Table.FirstOrDefaultAsync(sap => sap.DistributorSapNumber == request.DistributorSapNumber && sap.CompanyCode == request.CompanyCode);

            if (sapDistributorAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key);
            //Error-O-01 

            if (sapDistributorAccount.AccountType == AccountTypeEnum.BG.ToDescription())
                throw new NotFoundException(ErrorCodes.INVALID_ACCOUNT_TO_PLACE_ORDER.Value, ErrorCodes.INVALID_ACCOUNT_TO_PLACE_ORDER.Key);
            //Error-O-04

            if (sapDistributorAccount.CompanyCode != productDetail.CompanyCode)
                throw new NotFoundException(ErrorCodes.INVALID_COMPANYCODE_TO_PLACE_ORDER.Value, ErrorCodes.INVALID_COMPANYCODE_TO_PLACE_ORDER.Key);
            //Error-O-05

            var plant = await _plant.Table.FirstOrDefaultAsync(x => x.Code == request.PlantCode);
            if (plant == null)
                throw new NotFoundException(ErrorCodes.INVALID_PLANT_ERROR.Value, ErrorCodes.INVALID_PLANT_ERROR.Key);


            var shoppingCart = await _shoppingCartRepository.Table.Include(x => x.ShoppingCartItems).Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active)
                                                .FirstOrDefaultAsync();

            var estimatesRequest = new EstimateRequest
            {
                deliveryMethodCode = request.DeliveryMethodCode,
                companyCode = request.CompanyCode,
                countryCode = request.CountryCode,
                deliveryStateCode = request.DeliveryStateCode,
                distributorNumber = sapDistributorAccount.DistributorSapNumber,
                plantCode = request.PlantCode,
                quantity = request.Quantity.ToString(),
                unitOfMeasureCode = request.UnitOfMeasureCode,
                productId = productDetail.ProductSapNumber,
            };

            var getEstimate = await _sapService.GetItemEstimate(estimatesRequest);
            if (getEstimate.statusCode != "00")
                throw new NotFoundException(getEstimate.message, "");

            if (shoppingCart == null)
            {
                ShoppingCart addNewShoppingCart = new()
                {
                    UserId = LoggedInUserId,
                    CreatedByUserId = LoggedInUserId,
                    DateCreated = DateTime.UtcNow,
                    ShoppingCartStatusId = (int)ShoppingCartStatusEnum.Active,
                    DistributorSapAccountId = sapDistributorAccount.DistributorSapAccountId,
                    PlantCode = request.PlantCode,
                    DeliveryMethod = request.DeliveryMethodCode,
                    DateModified = DateTime.UtcNow,
                    ModifiedByUserId = LoggedInUserId,
                };
                await _shoppingCartRepository.AddAsync(addNewShoppingCart, cancellationToken);

                await _shoppingCartRepository.CommitAsync(cancellationToken);



                //Azure ServiceBus Orders.ShoppingCart.Created
                AddNewCartCreatedMessage newCartCreatedMessage = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = addNewShoppingCart.UserId,
                    CreatedByUserId = addNewShoppingCart.CreatedByUserId,
                    DateCreated = DateTime.UtcNow,
                    ShoppingCartId = addNewShoppingCart.Id,
                    ShoppingCartStatus = new StatusDto()
                    {
                        code = _shoppingCartStatusRepository.Table.Where(u => u.Id == addNewShoppingCart.ShoppingCartStatusId).FirstOrDefault()?.Code,
                        name = _shoppingCartStatusRepository.Table.Where(u => u.Id == addNewShoppingCart.ShoppingCartStatusId).FirstOrDefault()?.Name
                    },
                };

                await _messageBus.PublishTopicMessage(newCartCreatedMessage, EventMessages.ORDER_SHOPPINGCART_CREATED);

                var shoppingCartItemsi = await _shoppingCartItemRepository.Table.Where(sci => sci.ShoppingCartId == addNewShoppingCart.Id
                                                && sci.ProductId == request.ProductId && sci.DistributorSapAccountId == sapDistributorAccount.DistributorSapAccountId
                                                && sci.UnitOfMeasureCode == request.UnitOfMeasureCode).FirstOrDefaultAsync(cancellationToken);
                if (shoppingCartItemsi == null)
                {
                    ShoppingCartItem addNewiCart = new()
                    {
                        DistributorSapAccountId = sapDistributorAccount.DistributorSapAccountId,
                        CreatedByUserId = LoggedInUserId,
                        ShoppingCartId = addNewShoppingCart.Id,
                        DateCreated = DateTime.UtcNow,
                        Quantity = (decimal)request.Quantity,
                        ProductId = (short)request.ProductId,
                        ChannelCode = request.ChannelCode,
                        UnitOfMeasureCode = request.UnitOfMeasureCode,
                        PlantCode = request.PlantCode,
                        DeliveryMethodCode = request.DeliveryMethodCode,
                        DeliveryCountryCode = request.DeliveryCountryCode,
                        DeliveryStateCode = request.DeliveryStateCode,
                        DateModified = DateTime.UtcNow,
                        ModifiedByUserId = LoggedInUserId,
                    };
                    await _shoppingCartItemRepository.AddAsync(addNewiCart, cancellationToken);

                    await _shoppingCartItemRepository.CommitAsync(cancellationToken);
                }
                else
                {
                    var OldShoppingCartItems = shoppingCartItemsi;

                    shoppingCartItemsi.DistributorSapAccountId = sapDistributorAccount.DistributorSapAccountId;
                    shoppingCartItemsi.ModifiedByUserId = LoggedInUserId;
                    shoppingCartItemsi.DateModified = DateTime.UtcNow;
                    shoppingCartItemsi.ChannelCode = request.ChannelCode;
                    shoppingCartItemsi.DeliveryCountryCode = request.DeliveryCountryCode;
                    shoppingCartItemsi.DeliveryStateCode = request.DeliveryStateCode;
                    shoppingCartItemsi.Quantity += (decimal)request.Quantity;


                    await _shoppingCartItemRepository.UpdateAsync(shoppingCartItemsi);

                    _shoppingCartItemRepository.Commit();

                    //Azure ServiceBus Orders.ShoppingCart.Updated
                    var msgItems = new List<ShoppingCartItem11>();
                    foreach (var item in shoppingCart.ShoppingCartItems)
                    {
                        var product = _productRepository.Table.Where(x => x.Id.Equals(item.ProductId)).FirstOrDefault();
                        msgItems.Add(new ShoppingCartItem11
                        {
                            dateCreated = item.DateCreated,
                            Quantity = item.Quantity,
                            ShoppingCartItemId = item.Id,
                            UnitOfMeasureCode = item.UnitOfMeasureCode,
                            Product = product == null ? null : new Product11
                            {
                                Name = product.Name,
                                ProductId = item.Product.Id,
                                Price = product.Price,
                            }
                        });
                    }
                    NewCartUpdatedMessage newCartUpdatedMessage = new()
                    {
                        UserId = LoggedInUserId,
                        DateModified = shoppingCart.DateModified,
                        ModifiedByUserId = LoggedInUserId,
                        ShoppingCartId = OldShoppingCartItems.ShoppingCartId,
                        newShoppingCartStatus = new StatusDto
                        {
                            name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCart.Id).ShoppingCartStatusId)).FirstOrDefault().Name,
                            code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCart.Id).ShoppingCartStatusId)).FirstOrDefault().Code,

                        },
                        OldShoppingCartStatus = new StatusDto
                        {
                            name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                            code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,

                        },
                        DateCreated = shoppingCart.DateCreated,
                        Id = Guid.NewGuid(),
                        ShoppingCartItems = msgItems

                    };

                    await _messageBus.PublishTopicMessage(newCartUpdatedMessage, EventMessages.ORDER_SHOPPINGCART_UPDATED);

                }
            }
            else
            {
                if (shoppingCart.ShoppingCartItems.Count > 0)
                {
                    var initialDeliveryMethod = shoppingCart.ShoppingCartItems.First().DeliveryMethodCode;
                    if (initialDeliveryMethod.ToLower() != request.DeliveryMethodCode.ToLower())
                    {
                        throw new NotFoundException(ErrorCodes.MULTIPLE_DELIVERY_TYPE.Value, ErrorCodes.MULTIPLE_DELIVERY_TYPE.Key);
                    }
                }


                if (shoppingCart != null)
                {
                    var shoppingCartItems = await _shoppingCartItemRepository.Table.Where(sci => sci.ShoppingCartId == shoppingCart.Id
                                                    && sci.ProductId == request.ProductId
                                                    && sci.UnitOfMeasureCode == request.UnitOfMeasureCode).FirstOrDefaultAsync(cancellationToken);

                    if (shoppingCartItems == null)
                    {
                        ShoppingCartItem addNewiiCart = new()
                        {
                            DistributorSapAccountId = sapDistributorAccount.DistributorSapAccountId,
                            CreatedByUserId = LoggedInUserId,
                            ShoppingCartId = shoppingCart.Id,
                            DateCreated = DateTime.UtcNow,
                            Quantity = (decimal)request.Quantity,
                            ProductId = (short)request.ProductId,
                            ChannelCode = request.ChannelCode,
                            DeliveryStateCode = request.DeliveryStateCode,
                            DeliveryCountryCode = request.DeliveryCountryCode,
                            UnitOfMeasureCode = request.UnitOfMeasureCode,
                            PlantCode = request.PlantCode,
                            DeliveryMethodCode = request.DeliveryMethodCode,
                        };
                        await _shoppingCartItemRepository.AddAsync(addNewiiCart, cancellationToken);

                        await _shoppingCartItemRepository.CommitAsync(cancellationToken);
                    }
                    else
                    {
                        var OldShoppingCartItems = shoppingCartItems;


                        shoppingCartItems.DistributorSapAccountId = sapDistributorAccount.DistributorSapAccountId;
                        shoppingCartItems.ModifiedByUserId = LoggedInUserId;
                        shoppingCartItems.DateModified = DateTime.UtcNow;
                        shoppingCartItems.ChannelCode = request.ChannelCode;
                        shoppingCartItems.Quantity += (decimal)request.Quantity;

                        await _shoppingCartItemRepository.UpdateAsync(shoppingCartItems);

                        _shoppingCartItemRepository.Commit();

                        var shoppingCartUpdate = await _shoppingCartRepository.Table.Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active)
                                                        .FirstOrDefaultAsync(cancellationToken);

                        shoppingCart.DateModified = DateTime.UtcNow;
                        await _shoppingCartRepository.UpdateAsync(shoppingCartUpdate);
                        _shoppingCartRepository.Commit();

                        //Azure ServiceBus Orders.ShoppingCart.Updated
                        var msgItems = new List<ShoppingCartItem11>();
                        NewCartUpdatedMessage newCartUpdatedMessage = new()
                        {
                            UserId = LoggedInUserId,
                            DateModified = shoppingCartItems.DateModified,
                            ModifiedByUserId = LoggedInUserId,
                            ShoppingCartId = OldShoppingCartItems.ShoppingCartId,
                            newShoppingCartStatus = new StatusDto
                            {
                                name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                                code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,

                            },
                            OldShoppingCartStatus = new StatusDto
                            {
                                name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                                code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,

                            },
                            DateCreated = shoppingCart.DateCreated,
                            Id = Guid.NewGuid(),
                            ShoppingCartItems = msgItems

                        };

                        await _messageBus.PublishTopicMessage(newCartUpdatedMessage, EventMessages.ORDER_SHOPPINGCART_UPDATED);

                    }
                }

            }
            var originParams = new
            {
                plantCode = plant.Code,
                distributorSapNumber = request.DistributorSapNumber,
                deliveryMethodCode = request.DeliveryMethodCode,
                companyCode = request.CompanyCode,
                countryCode = request.CountryCode,
                deliveryCountryCode = request.DeliveryCountryCode,
                deliveryStateCode = request.DeliveryStateCode,
            };

            return ResponseHandler.SuccessResponse(SuccessMessages.NEW_PRODUCT_ADDED_SUCCESSFULLY_TO_CART, new { originParams = originParams });
        }
        public async Task<ApiResponse> UpdateProduct(UpdateProductItemToCartRequest request, int ShoppingCartItemId, CancellationToken cancellationToken)
        {
            GetUserId();

            if (ShoppingCartItemId != request.ShoppingCartItemId)
                throw new NotFoundException(ErrorCodes.SHOPPING_CART_NOTFOUND.Key, ErrorCodes.SHOPPING_CART_NOTFOUND.Value);

            var shoppingCartItem = await _shoppingCartItemRepository.Table.Where(sci => sci.Id == request.ShoppingCartItemId && sci.CreatedByUserId == LoggedInUserId)
                .FirstOrDefaultAsync();

            if (shoppingCartItem == null)
                throw new NotFoundException(ErrorCodes.SHOPPING_CART_NOTFOUND.Key, ErrorCodes.SHOPPING_CART_NOTFOUND.Value);

            var shoppingCart = await _shoppingCartRepository.Table.Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active)
                 .FirstOrDefaultAsync();
            var distributor = await _distributorSapAccountRepository.Table.FirstOrDefaultAsync(x => x.DistributorSapAccountId == shoppingCart.DistributorSapAccountId && x.UserId == LoggedInUserId);
            var product = await _productRepository.Table.FirstOrDefaultAsync(x => x.Id == shoppingCartItem.ProductId);
            //Error-O-06
            var OldShoppingCartItems = shoppingCartItem;
            if (request.Quantity <= 0.0)
            {
                _shoppingCartItemRepository.Delete(shoppingCartItem);
                _shoppingCartItemRepository.Commit();
                //Check if the item is the only item in the Cart

                var itemsInCart = await _shoppingCartItemRepository.Table.AnyAsync(x=>x.ShoppingCartId == shoppingCart.Id);
                if(!itemsInCart)
                {
                    _shoppingCartRepository.Table.Remove(shoppingCart);
                    _shoppingCartItemRepository.Commit();
                }
            }
            else
            {
                var estimatesRequest = new EstimateRequest
                {
                    deliveryMethodCode = shoppingCartItem.DeliveryMethodCode,
                    companyCode = distributor.CompanyCode,
                    countryCode = distributor.CountryCode,
                    deliveryStateCode = shoppingCartItem.DeliveryStateCode,
                    distributorNumber = distributor.DistributorSapNumber,
                    plantCode = shoppingCartItem.PlantCode,
                    quantity = request.Quantity.ToString(),
                    unitOfMeasureCode = shoppingCartItem.UnitOfMeasureCode,
                    productId = product.ProductSapNumber,
                };

                var getEstimate = await _sapService.GetItemEstimate(estimatesRequest);
                if (getEstimate.statusCode != "00")
                    throw new NotFoundException("Input quantity is not allowed for this product", "");


                shoppingCartItem.ModifiedByUserId = LoggedInUserId;
                shoppingCartItem.DateModified = DateTime.UtcNow;
                shoppingCartItem.Quantity = (decimal)request.Quantity;
                shoppingCartItem.SapEstimatedOrderValue = (decimal?)getEstimate.data.sapEstimate.orderValue;
                await _shoppingCartItemRepository.UpdateAsync(shoppingCartItem);

                await _shoppingCartItemRepository.CommitAsync(default);


                shoppingCart.DateModified = DateTime.UtcNow;
                await _shoppingCartRepository.UpdateAsync(shoppingCart);
                await _shoppingCartRepository.CommitAsync(default);
            }



            //Azure ServiceBus Orders.ShoppingCart.Updated
            var shoppingCart1 = await _shoppingCartRepository.Table.Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active).Include(x => x.ShoppingCartItems)
                    .FirstOrDefaultAsync();
            var msgItems = new List<ShoppingCartItem11>();
            foreach (var item in shoppingCart1.ShoppingCartItems)
            {
                var msgProduct = _productRepository.Table.FirstOrDefault(x => x.Id.Equals(item.ProductId));
                msgItems.Add(new ShoppingCartItem11
                {
                    dateCreated = item.DateCreated,
                    Quantity = item.Quantity,
                    ShoppingCartItemId = item.Id,
                    UnitOfMeasureCode = item.UnitOfMeasureCode,
                    Product = new Product11
                    {
                        Name = msgProduct.Name,
                        ProductId = item.ProductId,
                        Price = msgProduct.Price,
                    }
                });
            }
            NewCartUpdatedMessage newCartUpdatedMessage = new()
            {
                UserId = LoggedInUserId,
                DateModified = shoppingCartItem.DateModified,
                ModifiedByUserId = LoggedInUserId,
                ShoppingCartId = OldShoppingCartItems.ShoppingCartId,
                newShoppingCartStatus = new StatusDto
                {
                    name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCartItem.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                    code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCartItem.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,
                },
                OldShoppingCartStatus = new StatusDto
                {
                    name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                    code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,
                },
                DateCreated = shoppingCart1.DateCreated,
                Id = Guid.NewGuid(),
                ShoppingCartItems = msgItems
            };

            await _messageBus.PublishTopicMessage(newCartUpdatedMessage, EventMessages.ORDER_SHOPPINGCART_UPDATED);

            return ResponseHandler.SuccessResponse(SuccessMessages.NEW_PRODUCT_SUCCESSFULLY_UPDATED_IN_CART);
        }
        public async Task<ApiResponse> DeleteProduct(RemoveProductFromCartRequest request, int ShoppingCartItemId, CancellationToken cancellationToken)
        {
            GetUserId();

            if (ShoppingCartItemId != request.ShoppingCartItemId)
                throw new NotFoundException(ErrorCodes.SHOPPING_CART_NOTFOUND.Key, ErrorCodes.SHOPPING_CART_NOTFOUND.Value);

            var shoppingCartItems = await _shoppingCartItemRepository.Table.Where(sci => sci.Id == request.ShoppingCartItemId
                                                && sci.CreatedByUserId == LoggedInUserId)
                  .FirstOrDefaultAsync();

            if (shoppingCartItems == null)
                throw new NotFoundException(ErrorCodes.SHOPPING_CART_NOTFOUND.Key, ErrorCodes.SHOPPING_CART_NOTFOUND.Value);
            //Error-O-06

            _shoppingCartItemRepository.Delete(shoppingCartItems);

            var OldShoppingCartItems = shoppingCartItems;

            _shoppingCartItemRepository.Commit();

            var shoppingCart = await _shoppingCartRepository.Table.Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active)
                .FirstOrDefaultAsync();

            shoppingCart.DateModified = DateTime.UtcNow;
            await _shoppingCartRepository.UpdateAsync(shoppingCart);

            var shoppingCartItemList = await _shoppingCartItemRepository.Table.Where(x => x.ShoppingCartId == shoppingCart.Id).ToListAsync();
            if (shoppingCartItemList.Count == 0)
            {
                _shoppingCartRepository.Delete(shoppingCart);
            }
            else
            {

                var msgItems = new List<ShoppingCartItem11>();
                if (shoppingCart != null)
                {
                    foreach (var item in shoppingCart.ShoppingCartItems)
                    {
                        var product = _productRepository.Table.Where(x => x.Id.Equals(item.ProductId)).FirstOrDefault();

                        msgItems.Add(new ShoppingCartItem11
                        {
                            dateCreated = item.DateCreated,
                            Quantity = item.Quantity,
                            ShoppingCartItemId = item.Id,
                            UnitOfMeasureCode = item.UnitOfMeasureCode,
                            Product = product == null ? null : new Product11
                            {
                                Name = item.Product.Name,
                                ProductId = item.Product.Id,
                                Price = item.Product.Price,
                            }
                        });
                    }
                    NewCartUpdatedMessage newCartUpdatedMessage = new()
                    {
                        UserId = LoggedInUserId,
                        DateModified = shoppingCartItems.DateModified,
                        ModifiedByUserId = LoggedInUserId,
                        ShoppingCartId = OldShoppingCartItems.ShoppingCartId,
                        newShoppingCartStatus = new StatusDto
                        {
                            name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                            code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == shoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,

                        },
                        OldShoppingCartStatus = new StatusDto
                        {
                            name = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Name,
                            code = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == OldShoppingCartItems.ShoppingCartId).ShoppingCartStatusId)).FirstOrDefault().Code,

                        },
                        DateCreated = shoppingCart.DateCreated,
                        Id = Guid.NewGuid(),
                        ShoppingCartItems = msgItems

                    };

                    await _messageBus.PublishTopicMessage(newCartUpdatedMessage, EventMessages.ORDER_SHOPPINGCART_UPDATED);
                }
            }
            _shoppingCartRepository.Commit();

            //Azure ServiceBus Orders.ShoppingCart.Updated


            return ResponseHandler.SuccessResponse(SuccessMessages.NEW_PRODUCT_SUCCESSFULLY_DELETED_IN_CART);
        }
        public async Task<ApiResponse> GetActiveCarts(CancellationToken cancellationToken)
        {
            GetUserId();

            var key = $"{CacheKeys.SHOPPING_CART}{LoggedInUserId}";

            var cacheResponse = new ActiveCartResponse();

            if (cacheResponse.ShoppingCartItems == null)
            {
                var shoppingCartDetail = await _shoppingCartRepository.Table.Where(p => p.UserId == LoggedInUserId
                                                                    && p.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active).Select(x => new Shared.Data.Models.ShoppingCart
                                                                    {
                                                                        Id = x.Id,
                                                                        UserId = x.UserId,
                                                                        ShoppingCartStatusId = x.ShoppingCartStatusId,
                                                                        CreatedByUserId = x.CreatedByUserId,
                                                                        DateCreated = x.DateCreated,
                                                                        DateModified = x.DateModified,
                                                                        ModifiedByUserId = x.ModifiedByUserId,
                                                                        ShoppingCartStatus = new ShoppingCartStatus
                                                                        {
                                                                            Id = x.ShoppingCartStatus.Id,
                                                                            Name = x.ShoppingCartStatus.Name,
                                                                            Code = x.ShoppingCartStatus.Code
                                                                        },
                                                                        ShoppingCartItems = x.ShoppingCartItems.Select(i => new ShoppingCartItem
                                                                        {
                                                                            Id = i.Id,
                                                                            ShoppingCartId = i.ShoppingCartId,
                                                                            UnitOfMeasureCode = i.UnitOfMeasureCode,
                                                                            Quantity = i.Quantity,
                                                                            ProductId = i.ProductId,
                                                                            DistributorSapAccountId = i.DistributorSapAccountId,
                                                                            ChannelCode = i.ChannelCode,
                                                                            CreatedByUserId = i.CreatedByUserId,
                                                                            DateCreated = i.DateCreated,
                                                                            DateModified = i.DateModified,
                                                                            ModifiedByUserId = i.ModifiedByUserId,
                                                                            // i.Product.Price,
                                                                            //Product = new Product{ Id = i.Product.Id, Price = i.Product.Price,  Name = i.Product.Name},
                                                                            //DistributorSapAccount = new DistributorSapAccount { Id = i.DistributorSapAccount.Id, DistributorNumber = i.DistributorSapAccount.DistributorNumber, DistributorName = i.DistributorSapAccount.DistributorName }

                                                                        }).ToList()
                                                                    }).FirstOrDefaultAsync(cancellationToken);


                cacheResponse = ProcessQuery(shoppingCartDetail);
                if (cacheResponse == null)
                    return ResponseHandler.SuccessResponse("No record found...");

                ShoppingCartCacheDto cacheData = new(cacheResponse, LoggedInUserId);
                //await _cache.SetAsync(key, cacheData);
            }
            return ResponseHandler.SuccessResponse(SuccessMessages.ACTIVE_CARTLIST_SUCCESSFULLY, new { shoppingCart = cacheResponse });
        }
        public async Task<ApiResponse> GetActiveCartsV2(CancellationToken cancellationToken)
        {
            GetUserId();

            var walletBalanceEstimates = new List<object>();
            var totalEstimate = 0.0;

            var shoppingCartDetail = await _shoppingCartRepository.Table.Include(e => e.DistributorSapAccount).Include(sh => sh.ShoppingCartItems).Include(x => x.ShoppingCartStatus).Where(p => p.UserId == LoggedInUserId
                                                                && p.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active).FirstOrDefaultAsync(cancellationToken);
            if (shoppingCartDetail == null)
                return ResponseHandler.SuccessResponse("No record found...");

            var items = new List<ShoppingCartItemsResponse2>();
            foreach (var item in shoppingCartDetail.ShoppingCartItems)
            {
                var product = await _productRepository.Table.FirstOrDefaultAsync(y => y.Id.Equals(item.ProductId));

                //Call sap estimate Endpoint
                var distributor = await _distributorSapAccountRepository.Table.FirstOrDefaultAsync(x => x.DistributorSapAccountId == item.DistributorSapAccountId);
                var estimateRequestDto = new EstimateRequest();
                estimateRequestDto.distributorNumber = distributor.DistributorSapNumber;
                estimateRequestDto.quantity = item.Quantity.ToString();
                estimateRequestDto.unitOfMeasureCode = item.UnitOfMeasureCode;
                estimateRequestDto.deliveryMethodCode = shoppingCartDetail.DeliveryMethod;
                estimateRequestDto.productId = product.ProductSapNumber;
                estimateRequestDto.companyCode = distributor.CompanyCode;
                estimateRequestDto.countryCode = distributor.CountryCode;
                estimateRequestDto.plantCode = shoppingCartDetail.PlantCode;
                estimateRequestDto.deliveryStateCode = item.DeliveryStateCode;

                _cartLogger.LogInformation($"{"Get price Estimate:-"}{" | "}{JsonConvert.SerializeObject(estimateRequestDto)}");

                var estimate = await _sapService.GetItemEstimate(estimateRequestDto);
                if (estimate.statusCode == "00")
                {
                    item.SapEstimatedOrderValue = (decimal?)estimate.data.sapEstimate.orderValue + (decimal?)estimate.data.sapEstimate.vat + (decimal?)estimate.data.sapEstimate.freightCharges;
                    item.DateModified = DateTime.UtcNow;
                    item.DateOfOrderEstimate = DateTime.UtcNow;
                    await _shoppingCartItemRepository.CommitAsync(default);
                    totalEstimate += estimate.data.sapEstimate.orderValue;
                }
            }

            foreach (var item in shoppingCartDetail.ShoppingCartItems.GroupBy(x => x.DistributorSapAccountId))
            {
                var distributor1 = await _distributorSapAccountRepository.Table.FirstOrDefaultAsync(x => x.DistributorSapAccountId == item.Key);
                var sapWallet1 = await _sapService.GetWallet(distributor1.CompanyCode, distributor1.CountryCode, distributor1.DistributorSapNumber);

                if (sapWallet1 == null)
                    throw new NotFoundException(ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Key, ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Value);
                var totalEstimat1e = item.Sum(x => x.SapEstimatedOrderValue);

                var realBalance1 = sapWallet1.AvailableBalance < 0 ? Math.Abs(sapWallet1.AvailableBalance) : sapWallet1.AvailableBalance * -1;
                walletBalanceEstimates.Add(new
                {
                    distributorSapAccount = new DistributorSapAccountResponse(distributor1.DistributorSapAccountId, distributor1.DistributorSapNumber, distributor1.DistributorName),
                    sapWallet = realBalance1,
                    balanceAfterOrder = realBalance1 - (decimal)totalEstimat1e
                });

            }


            foreach (var item in shoppingCartDetail.ShoppingCartItems)
            {
                var product = await _productRepository.Table.FirstOrDefaultAsync(c => c.Id == item.ProductId);
                var distributor = await _distributorSapAccountRepository.Table.FirstOrDefaultAsync(x => x.DistributorSapAccountId == item.DistributorSapAccountId);
                var plant = await _plant.Table.FirstOrDefaultAsync(x => x.Code == item.PlantCode);

                items.Add(new ShoppingCartItemsResponse2
                {
                    estimatedValue = (decimal)item.SapEstimatedOrderValue,
                    DateCreated = item.DateCreated,
                    Quantity = item.Quantity,
                    ShoppingCartItemId = item.Id,
                    UnitOfMeasureCode = item.UnitOfMeasureCode,
                    Product = new CartProductResponse
                    {
                        Name = product.Name,
                        ProductId = product.Id,
                    },
                    SapEstimatedOrderValue = (decimal)item.SapEstimatedOrderValue,
                    DeliveryCountryCode = item.DeliveryCountryCode,
                    DeliveryStateCode = item.DeliveryStateCode,
                    DeliveryMethodCode = item.DeliveryMethodCode,
                    PlantCode = item.PlantCode,
                    PlantTypeCode = plant.PlantTypeCode,
                    DistributorSapAccount = new DistributorCartResponse(distributor.DistributorSapAccountId, distributor.DistributorSapNumber, distributor.DistributorName, distributor.CompanyCode)
                });

            }
            var cacheResponse = new ActiveCartResponse2()
            {
                ShoppingCartId = shoppingCartDetail.Id,
                CreatedByUserId = shoppingCartDetail.CreatedByUserId,
                DateCreated = shoppingCartDetail.DateCreated,
                DateModified = shoppingCartDetail.DateModified,
                UserId = shoppingCartDetail.UserId,
                ModifiedByUserId = (int)shoppingCartDetail.ModifiedByUserId,
                ShoppingCartStatus = new ShoppingCartStatusResponse(shoppingCartDetail.ShoppingCartStatus.Code, shoppingCartDetail.ShoppingCartStatus.Name),
                ShoppingCartItems = items,
                totalEstimatedValue = (decimal)totalEstimate,
            };


            ShoppingCartCacheDto2 cacheData = new(cacheResponse, LoggedInUserId);
            return ResponseHandler.SuccessResponse(SuccessMessages.ACTIVE_CARTLIST_SUCCESSFULLY, new { shoppingCart = cacheResponse, walletBalanceEstimates = walletBalanceEstimates });
        }
        public async Task<ApiResponse> AutoAbandonCarts(CancellationToken cancellationToken)
        {
            _cartLogger.LogInformation($"{"About to Abandon DMS Cart"}{" | "}{DateTime.UtcNow}");
            var shoppingCartRetentionDays = DateTime.UtcNow.AddDays(CacheKeys.RetentionDays);

            //var cartlist = _shoppingCartRepository.Table.Where(u => ((shoppingCartRetentionDays) < DateTime.UtcNow)
            //                            && u.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active).ToList();
            var cartlist = _shoppingCartRepository.Table.Where(u => (u.DateCreated < shoppingCartRetentionDays)
                                        && u.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active).ToList();

            foreach (var item in cartlist)
            {

                item.ShoppingCartStatusId = (int)ShoppingCartStatusEnum.Abandoned;
                item.DateModified = DateTime.UtcNow;

                await _shoppingCartRepository.UpdateAsync(item);
                _shoppingCartRepository.Commit();
                var cacheKey = $"{CacheKeys.SHOPPING_CART}{item.UserId}";
                await _cache.RemoveAsync(cacheKey);
                //Publish to Azure Bush

                //Azure ServiceBus Orders.DmsOrder.Abandoned
                OrdersShoppingCartAbandonedMessage dmsCartAbandonedMessage = new()
                {
                    UserId = item.UserId,
                    ShoppingCartStatus = _shoppingCartStatusRepository.Table.Where(u => u.Id == (_shoppingCartRepository.Table.FirstOrDefault(si => si.Id == item.Id).ShoppingCartStatusId)).FirstOrDefault().Name,
                    ModifiedByUserId = item.UserId,
                    ShoppingCartId = item.Id,
                    DateModified = item.DateModified,
                    ShoppingCartItems = (List<ShoppingCartItemsResponse>)item.ShoppingCartItems

                };
                _cartLogger.LogInformation($"{"End Abandon DMS Cart"}{" | "}{DateTime.UtcNow}");

                await _messageBus.PublishTopicMessage(dmsCartAbandonedMessage, EventMessages.DMSCART_ABANDONED);


            }
            _cartLogger.LogInformation($"{"About to Abandon DMS Cart"}{" | "}{DateTime.UtcNow}");
            return ResponseHandler.SuccessResponse(SuccessMessages.SHOPPINGCARTS_SUCCESSFULLY_ABANDONED);
        }
        private ActiveCartResponse ProcessQuery(Shared.Data.Models.ShoppingCart cart)
        {
            if (cart == null)
                return null;

            var cartResponse = new ActiveCartResponse()
            {
                UserId = cart.UserId,
                DateModified = cart.DateModified,
                DateCreated = cart.DateCreated,
                ShoppingCartId = cart.Id,
                CreatedByUserId = cart.UserId,
                ModifiedByUserId = cart.UserId,
                ShoppingCartStatus = new ShoppingCartStatusResponse(cart.ShoppingCartStatus.Code, cart.ShoppingCartStatus.Name),
                ShoppingCartItems = !cart.ShoppingCartItems.Any() ? new List<ShoppingCartItemsResponse>() : cart.ShoppingCartItems.Select(x => new ShoppingCartItemsResponse
                {
                    DateCreated = x.DateCreated,
                    Quantity = x.Quantity,
                    ShoppingCartItemId = x.Id,
                    UnitOfMeasureCode = x.UnitOfMeasureCode,
                    Product = new ProductResponse(_productRepository.Table.Where(c => c.Id == x.ProductId).FirstOrDefault().Id, _productRepository.Table.Where(c => c.Id == x.ProductId).FirstOrDefault().Price, _productRepository.Table.Where(c => c.Id == x.ProductId).FirstOrDefault().Name),
                    DistributorSapAccount = new DistributorSapAccountResponse(_distributorSapAccountRepository.Table.Where(c => c.DistributorSapAccountId == x.DistributorSapAccountId).FirstOrDefault().DistributorSapAccountId,
                    _distributorSapAccountRepository.Table.Where(c => c.DistributorSapAccountId == x.DistributorSapAccountId).FirstOrDefault().DistributorSapNumber,
                    _distributorSapAccountRepository.Table.Where(c => c.DistributorSapAccountId == x.DistributorSapAccountId).FirstOrDefault().DistributorName)
                }).ToList()
            };
            return cartResponse;
        }

    }
}
