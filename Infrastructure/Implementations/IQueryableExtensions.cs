using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPDomain.HelperModels;

namespace TMPInfrastructure.Implementations
{
    public static class IQueryableExtensions
    {
        public static async Task<PagedResult<T>> GetPagedAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            var result = new PagedResult<T>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = await query.CountAsync()
            };

            result.Items = await query.Skip((pageNumber - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            return result;
        }
    }
}
