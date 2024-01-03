using Order.Application.DTOs.Filters;
using Shared.Data.Extensions;
using Shared.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Infrastructure.QueryObjects
{
    public class DmsOrderDistrubutorQueryObject : QueryObject<DmsOrder>
    {
        public DmsOrderDistrubutorQueryObject(RequestFilterDto filter)
        {
            if (filter == null) return;

            if (!string.IsNullOrWhiteSpace(filter.CountryCode))
                And(u => u.CountryCode == filter.CountryCode);

            if (!string.IsNullOrWhiteSpace(filter.CompanyCode))
                And(u => u.CompanyCode == filter.CompanyCode);

            if (filter.UserId != 0)
                And(u => u.UserId == filter.UserId);

            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                And(u =>
                //u.CompanyCode.ToLower().Contains(filter.SearchKeyword.ToLower()) ||
                //u.OrderType.Code == null || u.OrderType.Code.Contains(filter.SearchKeyword.ToLower()) ||
                //string.IsNullOrEmpty(u.Plant.Name) || u.PlantCode.ToLower().Contains(filter.SearchKeyword.ToLower()) ||
                //u.OrderType.Code == null || u.OrderType.Code.ToLower().Contains(filter.SearchKeyword.ToLower()) ||
                //u.OrderStatus == null || u.OrderStatus.Name.ToLower().Contains(filter.SearchKeyword.ToLower()) ||
                !string.IsNullOrEmpty(u.DeliveryAddress) && u.DeliveryAddress.ToLower().Contains(filter.SearchKeyword.ToLower()) ||
                //!string.IsNullOrEmpty(u.DeliveryCity) && u.DeliveryCity.ToLower().Contains(filter.SearchKeyword.ToLower()) ||
                !string.IsNullOrEmpty(u.DeliveryStateCode) && u.DeliveryStateCode.ToLower().Contains(filter.SearchKeyword.ToLower()) ||
                !string.IsNullOrEmpty(u.DistributorSapAccount.DistributorName) && u.DistributorSapAccount.DistributorName.ToLower().Contains(filter.SearchKeyword.ToLower()) ||
                !string.IsNullOrEmpty(u.DistributorSapAccount.DistributorSapNumber) && u.DistributorSapAccount.DistributorSapNumber.Equals(filter.SearchKeyword) ||
                !string.IsNullOrEmpty(u.DeliveryCity) && u.DeliveryCity.ToLower().Contains(filter.SearchKeyword.ToLower().Trim()) ||
                !string.IsNullOrEmpty(u.OrderSapNumber) && u.OrderSapNumber.ToLower().Contains(filter.SearchKeyword.ToLower().Trim())
                
                ) ;
            }
            if (!string.IsNullOrWhiteSpace(filter.OrderStatusCode))
                And(u => u.OrderStatus.Code == filter.OrderStatusCode);

            if (!string.IsNullOrWhiteSpace(filter.OrderTypeCode))
                And(u => u.OrderType.Code.ToLower().Trim() == filter.OrderTypeCode.ToLower().Trim());

            if (!string.IsNullOrWhiteSpace(filter.DeliveryMethodCode))
                And(u => u.DeliveryMethodCode == filter.DeliveryMethodCode);

            if (!string.IsNullOrWhiteSpace(filter.OrderSapNumber))
                And(u => u.OrderSapNumber == filter.OrderSapNumber);

            if (filter.DistributorSapAccountId > 0)
                And(u => u.DistributorSapAccountId == filter.DistributorSapAccountId);

            if (filter.IsATC)
                And(u => u.IsAtc == filter.IsATC);

            if (!string.IsNullOrWhiteSpace(Convert.ToString(filter.FromDate)) && !string.IsNullOrWhiteSpace(Convert.ToString(filter.ToDate)))
                And(u => u.DateCreated.Date >= filter.FromDate.Value.Date && u.DateCreated.Date <= filter.ToDate.Value.Date);
        }
    }
}
