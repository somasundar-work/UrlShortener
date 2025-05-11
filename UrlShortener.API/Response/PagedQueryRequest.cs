using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FastEndpoints;

namespace UrlShortener.API.Response
{
    public class PagedQueryRequest
    {
        [QueryParam]
        public int PageNumber { get; set; } = 1;

        [QueryParam]
        public int PageSize { get; set; } = 10;

        [QueryParam]
        public string? SortBy { get; set; }

        [QueryParam]
        public string? SortDirection { get; set; } = "asc";

        [QueryParam]
        public string? SearchTerm { get; set; }

        [QueryParam]
        public Dictionary<string, string>? Filters { get; set; }
    }
}
