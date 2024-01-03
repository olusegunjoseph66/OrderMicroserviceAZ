using Order.Application.DTOs.Filters;
using Shared.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Infrastructure.QueryObjects
{

    public class PlantQueryObject : QueryObject<Shared.Data.Models.Plant>
    {
        public PlantQueryObject(PlantFilterDto filter)
        {
            if (filter == null) return;

            if (!string.IsNullOrWhiteSpace(filter.CompanyCode))
                And(u => u.CompanyCode == filter.CompanyCode);

            if (!string.IsNullOrWhiteSpace(filter.CountryCode))
                And(u => u.CountryCode == filter.CountryCode);


        }
    }
}
