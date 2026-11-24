using MomRecipes.Application.Pagination;
using MomRecipes.Domain.Entities;

namespace MomRecipes.Application.Catalog;

public interface ICatalogRepository
{
    Task<PagedResult<IngredientDto>> ListIngredientsAsync(PageRequest request, CancellationToken cancellationToken);
    Task<PagedResult<TagDto>> ListTagsAsync(PageRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<Ingredient>> FindIngredientsAsync(int[] ids, string[] names, CancellationToken cancellationToken);
    Task<IReadOnlyList<Tag>> FindTagsAsync(int[] ids, string[] names, CancellationToken cancellationToken);
}
