using System.Collections.Concurrent;
using MomRecipes.Application.Recipes;
using MomRecipes.Domain.Entities;

namespace MomRecipes.Infrastructure.Recipes;

public sealed class InMemoryRecipeRepository : IRecipeRepository
{
    private readonly ConcurrentDictionary<Guid, Recipe> _recipes = new();

    public Task AddAsync(Recipe recipe, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_recipes.TryAdd(recipe.Id, recipe))
        {
            throw new InvalidOperationException($"Recipe '{recipe.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_recipes.GetValueOrDefault(id));
    }

    public Task<IReadOnlyList<Recipe>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Recipe> recipes = _recipes.Values
            .OrderBy(recipe => recipe.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(recipe => recipe.Id)
            .ToArray();

        return Task.FromResult(recipes);
    }
}
