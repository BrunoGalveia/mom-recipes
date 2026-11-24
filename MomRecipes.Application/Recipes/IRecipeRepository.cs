using MomRecipes.Application.Pagination;
using MomRecipes.Domain.Entities;

namespace MomRecipes.Application.Recipes;

public interface IRecipeRepository
{
    Task AddAsync(Recipe recipe, CancellationToken cancellationToken);
    Task<Recipe?> ReplaceAsync(Guid id, Recipe replacement, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<Recipe>> SearchAsync(RecipeSearchRequest request, CancellationToken cancellationToken);
}
