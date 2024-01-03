using Order.Application.DTOs.Filters;
using Shared.Data.Extensions;

namespace Order.Infrastructure.QueryObjects
{
    public class TruckSizesQueryObject : QueryObject<Shared.Data.Models.TruckSizeDeliveryMethodMapping>
    {
        public TruckSizesQueryObject(TruckSizesFilterDto filter)
        {
            if (filter == null) return;

            if (!string.IsNullOrWhiteSpace(filter.DeliveryMethodCode))
                And(u => u.DeliveryMethodCode == filter.DeliveryMethodCode);

            if (!string.IsNullOrWhiteSpace(filter.PlantTypeCode))
                And(u => u.PlantTypeCode == filter.PlantTypeCode);
        }
    }
}
