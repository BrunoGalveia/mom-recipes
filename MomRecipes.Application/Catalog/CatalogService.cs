using MomRecipes.Application.Pagination;

namespace MomRecipes.Application.Catalog;

public sealed class CatalogService(ICatalogRepository repository)
{
    public Task<PagedResult<IngredientDto>> ListIngredientsAsync(
        PageRequest request, CancellationToken cancellationToken)
    {
        return repository.ListIngredientsAsync(request, cancellationToken);
    }

    public Task<PagedResult<TagDto>> ListTagsAsync(PageRequest request, CancellationToken cancellationToken)
    {
        return repository.ListTagsAsync(request, cancellationToken);
    }
}
