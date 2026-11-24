using Microsoft.EntityFrameworkCore;
using MomRecipes.Application.Pagination;
using MomRecipes.Application.Recipes;
using MomRecipes.Domain.Entities;
using MomRecipes.Infrastructure.Persistence;

namespace MomRecipes.Infrastructure.Recipes;

internal sealed class RecipeRepository(MomRecipesDbContext dbContext) : IRecipeRepository
{
    public async Task AddAsync(Recipe recipe, CancellationToken cancellationToken)
    {
        TrackCatalogReferences(recipe);
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Recipe?> ReplaceAsync(Guid id, Recipe replacement, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Lock the recipe before reading its children so updates and deletes cannot interleave.
        var lockedRecipes = await dbContext.Recipes
            .FromSqlInterpolated($"""SELECT * FROM "Recipes" WHERE "Id" = {id} FOR UPDATE""")
            .ToListAsync(cancellationToken);
        var recipe = lockedRecipes.SingleOrDefault();
        if (recipe is null)
        {
            return null;
        }

        TrackCatalogReferences(replacement);
        var entry = dbContext.Entry(recipe);
        await entry.Collection(entity => entity.Ingredients).LoadAsync(cancellationToken);
        await entry.Collection(entity => entity.Steps).LoadAsync(cancellationToken);
        await entry.Collection(entity => entity.RecipeTags).LoadAsync(cancellationToken);

        recipe.ReplaceContents(replacement);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return recipe;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.Recipes.Where(recipe => recipe.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        return affectedRows != 0;
    }

    private void TrackCatalogReferences(Recipe recipe)
    {
        foreach (var link in recipe.Ingredients)
        {
            var ingredient = link.Ingredient;
            if (ingredient.Id != 0)
            {
                var entry = dbContext.Entry(ingredient);
                if (entry.State == EntityState.Detached)
                {
                    entry.State = EntityState.Unchanged;
                }
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

    }

    public Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return IncludeDetails(dbContext.Recipes).AsSingleQuery()
            .SingleOrDefaultAsync(recipe => recipe.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Recipe>> SearchAsync(
        RecipeSearchRequest request, CancellationToken cancellationToken)
    {
        var offset = request.GetOffset();
        IQueryable<Recipe> query = dbContext.Recipes;
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var escapedName = request.Name.Trim()
                .Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var pattern = $"%{escapedName}%";
            query = query.Where(recipe => EF.Functions.ILike(recipe.Title, pattern, "\\"));
        }

        if (request.MinTimeInMinutes is int min)
        {
            query = query.Where(recipe => recipe.TotalTimeInMinutes >= min);
        }
        if (request.MaxTimeInMinutes is int max)
        {
            query = query.Where(recipe => recipe.TotalTimeInMinutes <= max);
        }

        foreach (var ingredientId in request.IngredientIds.Distinct())
        {
            query = query.Where(recipe => recipe.Ingredients.Any(link => link.IngredientId == ingredientId));
        }
        foreach (var name in NormalizeNames(request.IngredientNames))
        {
            query = query.Where(recipe => recipe.Ingredients.Any(link => link.Ingredient.Name.ToLower() == name));
        }
        foreach (var tagId in request.TagIds.Distinct())
        {
            query = query.Where(recipe => recipe.RecipeTags.Any(link => link.TagId == tagId));
        }
        foreach (var name in NormalizeNames(request.TagNames))
        {
            query = query.Where(recipe => recipe.RecipeTags.Any(link => link.Tag.Name.ToLower() == name));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await IncludeDetails(query)
            .OrderBy(recipe => recipe.Title.ToLower())
            .ThenBy(recipe => recipe.Id)
            .Skip(offset)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<Recipe>(items, request.Page, request.PageSize, totalCount);
    }

    private static IEnumerable<string> NormalizeNames(IEnumerable<string> names)
    {
        return names.Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim().ToLowerInvariant()).Distinct();
    }

    private static IQueryable<Recipe> IncludeDetails(IQueryable<Recipe> query)
    {
        return query
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
            var entry = dbContext.Entry(tag);
            if (entry.State == EntityState.Detached)
            {
                entry.State = EntityState.Unchanged;
            }
        }
    }
}
