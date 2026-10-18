using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace GamingHub.Api.Data.Paging
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        // routeValues = extra values the link needs: route ids (roomId, stationId) and filters (genre, status…)
        public string? GetPreviousPageLink(LinkGenerator linkGenerator, HttpContext httpContext, 
                                           string endpointName, object? routeValues = null)
        {
            return HasPrevious ? linkGenerator.GetUriByName(httpContext, endpointName, BuildRouteValues(routeValues, CurrentPage - 1)) : null;
        }
        public string? GetNextPageLink(LinkGenerator linkGenerator, HttpContext httpContext, 
                                       string endpointName, object? routeValues = null)
        {
            return HasNext
                ? linkGenerator.GetUriByName(httpContext, endpointName, BuildRouteValues(routeValues, CurrentPage + 1))
                : null;
        }

        public PaginationMetadata CreatePaginationMetadata(LinkGenerator linkGenerator, HttpContext httpContext, 
                                                           string endpointName, object? routeValues = null)
        {
            return new PaginationMetadata(TotalCount, PageSize, CurrentPage, TotalPages,
                GetPreviousPageLink(linkGenerator, httpContext, endpointName, routeValues),
                GetNextPageLink(linkGenerator, httpContext, endpointName, routeValues));
        }

        private RouteValueDictionary BuildRouteValues(object? routeValues, int pageNumber)
        {
            var values = new RouteValueDictionary(routeValues);
            values["pageNumber"] = pageNumber;
            values["pageSize"] = PageSize;
            return values;
        }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = count;

            AddRange(items);
        }
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
