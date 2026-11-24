using Microsoft.EntityFrameworkCore;
using MomRecipes.Application.Recipes;
using MomRecipes.Domain.Entities;
using MomRecipes.Infrastructure.Persistence;

namespace MomRecipes.Infrastructure.Recipes;

internal sealed class RecipeRepository(MomRecipesDbContext dbContext) : IRecipeRepository
{
    public async Task AddAsync(Recipe recipe, CancellationToken cancellationToken)
    {
        foreach (var link in recipe.Ingredients)
        {
            var ingredient = link.Ingredient;
            if (ingredient.Id != 0)
            {
                dbContext.Entry(ingredient).State = EntityState.Unchanged;
                continue;
            }

            foreach (var ingredientTag in ingredient.IngredientTags)
            {
                TrackExistingTag(ingredientTag.Tag);
            }
        }

        foreach (var recipeTag in recipe.RecipeTags)
        {
            TrackExistingTag(recipeTag.Tag);
        }

        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return QueryRecipes().SingleOrDefaultAsync(recipe => recipe.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Recipe>> ListAsync(CancellationToken cancellationToken)
    {
        return await QueryRecipes()
            .OrderBy(recipe => recipe.Title.ToLower())
            .ThenBy(recipe => recipe.Id)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Recipe> QueryRecipes()
    {
        return dbContext.Recipes
            .AsNoTrackingWithIdentityResolution()
            .AsSplitQuery()
            .Include(recipe => recipe.Steps.OrderBy(step => step.Position))
            .Include(recipe => recipe.Ingredients.OrderBy(ingredient => ingredient.Id))
                .ThenInclude(link => link.Ingredient)
                    .ThenInclude(ingredient => ingredient.IngredientTags)
                        .ThenInclude(link => link.Tag)
            .Include(recipe => recipe.RecipeTags)
                .ThenInclude(link => link.Tag);
    }

    private void TrackExistingTag(Tag tag)
    {
        if (tag.Id != 0)
        {
            dbContext.Entry(tag).State = EntityState.Unchanged;
        }
    }
}
