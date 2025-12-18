// using Microsoft.EntityFrameworkCore;
//
// namespace CoffeeShopApi.Shared.Pagination
// {
//     public class PaginatedList<T>
//     {
//         public List<T> Items { get; set; }
//         public int PageIndex { get; private set; }
//         public int TotalPages { get; private set; }
//         public int TotalCount { get; private set; }
//         public int PageSize { get; private set; }
//
//         public bool HasPreviousPage => PageIndex > 1;
//         public bool HasNextPage => PageIndex < TotalPages;
//
//         public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
//         {
//             PageIndex = pageIndex;
//             TotalCount = count;
//             PageSize = pageSize;
//             TotalPages = (int)Math.Ceiling(count / (double)pageSize);
//             Items = items;
//         }
//
//         // Phương thức tĩnh để tạo phân trang bất đồng bộ (Async)
//         // IQueryable<T> source: Câu lệnh truy vấn (chưa thực thi) từ EF Core
//         public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
//         {
//             // 1. Đếm tổng số bản ghi (Query Count)
//             var count = await source.CountAsync();
//
//             // 2. Lấy dữ liệu theo trang (Query Skip/Take)
//             var items = await source.Skip((pageIndex - 1) * pageSize)
//                 .Take(pageSize)
//                 .ToListAsync();
//
//             return new PaginatedList<T>(items, count, pageIndex, pageSize);
//         }
//     }
// }