namespace MedApp.Application.Shared
{
    public record PagedResult<T>(
        IEnumerable<T> Items,
        int Total,
        int Page,
        int PageSize
       );
}
