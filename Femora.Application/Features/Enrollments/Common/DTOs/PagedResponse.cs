namespace Femora.Application.Features.Enrollments.Common.DTOs;

public class PagedResponse<T>
{
    public List<T> Data { get; init; } = new();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;
}
