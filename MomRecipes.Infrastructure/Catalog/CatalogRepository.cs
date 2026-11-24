using Microsoft.EntityFrameworkCore;
using MomRecipes.Application.Catalog;
using MomRecipes.Application.Pagination;
using MomRecipes.Domain.Entities;
using MomRecipes.Infrastructure.Persistence;

namespace MomRecipes.Infrastructure.Catalog;

internal sealed class CatalogRepository(MomRecipesDbContext dbContext) : ICatalogRepository
{
    public Task<PagedResult<IngredientDto>> ListIngredientsAsync(
        PageRequest request, CancellationToken cancellationToken)
    {
        return ReadPageAsync(dbContext.Ingredients.AsNoTracking()
            .OrderBy(ingredient => ingredient.Name.ToLower()).ThenBy(ingredient => ingredient.Id)
            .Select(ingredient => new IngredientDto(ingredient.Id, ingredient.Name, ingredient.ImageUrl)),
            request, cancellationToken);
    }

    public Task<PagedResult<TagDto>> ListTagsAsync(PageRequest request, CancellationToken cancellationToken)
    {
        return ReadPageAsync(dbContext.Tags.AsNoTracking()
            .OrderBy(tag => tag.Name.ToLower()).ThenBy(tag => tag.Id)
            .Select(tag => new TagDto(tag.Id, tag.Name)), request, cancellationToken);
    }

    public async Task<IReadOnlyList<Ingredient>> FindIngredientsAsync(
        int[] ids, string[] names, CancellationToken cancellationToken)
    {
        if (ids.Length == 0 && names.Length == 0)
        {
            return [];
        }

        return await dbContext.Ingredients
            .Where(ingredient => ids.Contains(ingredient.Id) || names.Contains(ingredient.Name.ToLower()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> FindTagsAsync(
        int[] ids, string[] names, CancellationToken cancellationToken)
    {
        if (ids.Length == 0 && names.Length == 0)
        {
            return [];
        }

        return await dbContext.Tags
            .Where(tag => ids.Contains(tag.Id) || names.Contains(tag.Name.ToLower()))
            .ToListAsync(cancellationToken);
    }

    private static async Task<PagedResult<T>> ReadPageAsync<T>(
        IQueryable<T> query, PageRequest request, CancellationToken cancellationToken)
    {
        var offset = request.GetOffset();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(offset).Take(request.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, request.Page, request.PageSize, totalCount);
    }
}
