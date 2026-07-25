using System.Collections.Generic;

namespace Femora.Application.Common.Models;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
