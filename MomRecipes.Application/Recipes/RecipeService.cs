using MomRecipes.Application.Catalog;
using MomRecipes.Application.Pagination;
using MomRecipes.Domain.Entities;

namespace MomRecipes.Application.Recipes;

public sealed class RecipeService(IRecipeRepository repository, ICatalogRepository catalogRepository)
{
    public async Task<RecipeDto> CreateAsync(SaveRecipeRequest request, CancellationToken cancellationToken)
    {
        var recipe = await BuildRecipeAsync(request, cancellationToken);
        await repository.AddAsync(recipe, cancellationToken);
        return ToDto(recipe);
    }

    public async Task<RecipeDto?> UpdateAsync(
        Guid id, SaveRecipeRequest request, CancellationToken cancellationToken)
    {
        var replacement = await BuildRecipeAsync(request, cancellationToken);
        var recipe = await repository.ReplaceAsync(id, replacement, cancellationToken);
        return recipe is null ? null : ToDto(recipe);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }

    private async Task<Recipe> BuildRecipeAsync(SaveRecipeRequest request, CancellationToken cancellationToken)
    {
        var recipe = new Recipe(
            request.Title, request.Description, request.Servings, request.ImageUrl, request.TotalTimeInMinutes);

        var ingredients = request.Ingredients ?? [];
        var ingredientIds = ingredients
            .Select(ingredient => ingredient?.IngredientId)
            .OfType<int>()
            .Distinct()
            .ToArray();
        var ingredientNames = ingredients
            .Where(item => item is { IngredientId: null } && !string.IsNullOrWhiteSpace(item.Name))
            .Select(item => item!.Name!.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();
        var catalogIngredients = await catalogRepository.FindIngredientsAsync(
            ingredientIds, ingredientNames, cancellationToken);
        var ingredientsById = catalogIngredients.ToDictionary(ingredient => ingredient.Id);

        // Repeated occurrences can have different quantities and notes.
        var ingredientsByName = new Dictionary<string, Ingredient>(StringComparer.OrdinalIgnoreCase);
        foreach (var ingredient in catalogIngredients.OrderBy(ingredient => ingredient.Id))
        {
            ingredientsByName.TryAdd(ingredient.Name, ingredient);
        }
        foreach (var item in ingredients)
        {
            ArgumentNullException.ThrowIfNull(item, nameof(request.Ingredients));
            Ingredient ingredient;
            if (item.IngredientId is int ingredientId)
            {
                ingredient = ingredientsById.GetValueOrDefault(ingredientId)
                    ?? throw new ArgumentException(
                        $"Ingredient {ingredientId} does not exist.", nameof(request.Ingredients));
            }
            else
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(item.Name, nameof(item.Name));
                var name = item.Name.Trim();
                if (!ingredientsByName.TryGetValue(name, out var match))
                {
                    match = new Ingredient(name);
                    ingredientsByName.Add(name, match);
                }

                ingredient = match;
            }

            recipe.AddIngredient(ingredient, item.Quantity, item.Unit, item.Notes);
        }

        foreach (var instruction in request.Steps ?? [])
        {
            recipe.AddStep(instruction);
        }

        await AddTagsAsync(recipe, request, cancellationToken);
        return recipe;
    }

    public async Task<RecipeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var recipe = await repository.GetByIdAsync(id, cancellationToken);
        return recipe is null ? null : ToDto(recipe);
    }

    public async Task<PagedResult<RecipeDto>> SearchAsync(
        RecipeSearchRequest request, CancellationToken cancellationToken)
    {
        if (request.MinTimeInMinutes is < 0 || request.MaxTimeInMinutes is < 0 ||
            request.MinTimeInMinutes > request.MaxTimeInMinutes)
        {
            throw new ArgumentException("The time range must be non-negative and ordered.");
        }

        if (request.IngredientIds.Any(id => id <= 0) || request.TagIds.Any(id => id <= 0))
        {
            throw new ArgumentException("Ingredient and tag IDs must be positive.");
        }

        var recipes = await repository.SearchAsync(request, cancellationToken);
        return new PagedResult<RecipeDto>(
            recipes.Items.Select(ToDto).ToArray(), recipes.Page, recipes.PageSize, recipes.TotalCount);
    }

    private async Task AddTagsAsync(Recipe recipe, SaveRecipeRequest request, CancellationToken cancellationToken)
    {
        var ids = (request.TagIds ?? []).Distinct().ToArray();
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in request.TagNames ?? [])
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(request.TagNames));
            var trimmed = name.Trim();
            names.TryAdd(trimmed.ToLowerInvariant(), trimmed);
        }

        var tags = await catalogRepository.FindTagsAsync(ids, names.Keys.ToArray(), cancellationToken);
        var tagsById = tags.ToDictionary(tag => tag.Id);
        foreach (var id in ids)
        {
            if (!tagsById.TryGetValue(id, out var tag))
            {
                throw new ArgumentException($"Tag {id} does not exist.", nameof(request.TagIds));
            }
            recipe.AddTag(tag);
        }

        var tagsByName = tags.OrderByDescending(tag => ids.Contains(tag.Id)).ThenBy(tag => tag.Id)
            .ToLookup(tag => tag.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var name in names.Values)
        {
            recipe.AddTag(tagsByName[name].FirstOrDefault() ?? new Tag(name));
        }
    }

    private static RecipeDto ToDto(Recipe recipe) => new(
        recipe.Id,
        recipe.Title,
        recipe.Description,
        recipe.Servings,
        recipe.Ingredients.Select(ingredient => new RecipeIngredientDto(
            ingredient.Ingredient.Name, ingredient.Quantity, ingredient.Unit,
            ingredient.IngredientId, ingredient.Notes)).ToArray(),
        recipe.Steps.Select(step => new RecipeStepDto(step.Position, step.Instruction)).ToArray(),
        recipe.ImageUrl,
        recipe.TotalTimeInMinutes,
        recipe.RecipeTags.OrderBy(link => link.Tag.Name, StringComparer.OrdinalIgnoreCase).ThenBy(link => link.TagId)
            .Select(link => new TagDto(link.TagId, link.Tag.Name)).ToArray());
}
