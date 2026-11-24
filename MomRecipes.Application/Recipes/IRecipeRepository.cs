using MomRecipes.Domain.Entities;

namespace MomRecipes.Application.Recipes;

public interface IRecipeRepository
{
    Task AddAsync(Recipe recipe, CancellationToken cancellationToken);
    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Recipe>> ListAsync(CancellationToken cancellationToken);
}
