namespace CoffeeShopApi.DTOs
{
    // Thêm <T> để class này có thể chứa bất kỳ loại object nào (ProductDto, UserDto...)
    public class PaginatedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<string, int>? Count { get; set; }
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Count per status or custom groups. Key = status name, Value = count
        /// </summary>
        

        public PaginatedResponse(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }
    }
}