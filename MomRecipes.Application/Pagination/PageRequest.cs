namespace MomRecipes.Application.Pagination;

public class PageRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public int GetOffset()
    {
        if (PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(PageSize), "Page size must be between 1 and 100.");
        }

        if (Page < 1 || (long)(Page - 1) * PageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(Page), "Page number is out of range.");
        }

        return (Page - 1) * PageSize;
    }
}
