namespace UrlShortener.API.Response
{
    public class PaginatedResponse<T>(IEnumerable<T> data, int count, int pageNumber, int pageSize)
    {
        public int PageNumber { get; set; } = pageNumber;
        public int PageSize { get; set; } = pageSize;
        public int TotalRecords { get; set; } = count;
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
        public IEnumerable<T> Data { get; set; } = data;

        public static PaginatedResponse<T> CreatePage(IEnumerable<T> data, int pageNumber, int pageSize)
        {
            int end = pageNumber * pageSize;
            int start = end - pageSize;
            int count = data.Count();
            var result = data.Take(start..end);
            return new(result, count, pageNumber, pageSize);
        }
    }
}
