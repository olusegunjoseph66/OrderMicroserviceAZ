using AutoMapper;
using Order.Application.DTOs.Events;
using Order.Application.DTOs.Request;
using Order.Application.DTOs.Response;
using Shared.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.AutoMapperSettings
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<DmsOrder, GetMyDMSOrderDto>();
                config.CreateMap<DmsOrder, GetMySAPOrderDto>().ReverseMap();
                config.CreateMap<DmsOrder, ViewDmsOrderDetailsDto>().ReverseMap();
                config.CreateMap<DmsOrder, ViewSapOrderDetailsDtoVM>().ReverseMap();
                config.CreateMap<DmsOrder, GetMySAPChildOrderDto>().ReverseMap();
                config.CreateMap<DmsOrder, GetAdminDMSOrderDto>().ReverseMap();
                config.CreateMap<DmsOrder, DmsRequestDTO>().ReverseMap();
                config.CreateMap<TruckSize, TruckSizeDto>().ReverseMap();
                config.CreateMap<DeliveryMethod, DeliveryMethodDto>().ReverseMap();
                config.CreateMap<Product, ProductDto>().ReverseMap(); 
                config.CreateMap<Product, ProductRefreshedMessage>().ReverseMap();
                config.CreateMap<DmsOrder, DmsOrderDto>().ReverseMap();
                config.CreateMap<Shared.ExternalServices.DTOs.APIModels.SapOrderDto, DmsOrder>().ReverseMap();


            });


            return mappingConfig;
        }
    }
}
