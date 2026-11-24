using MomRecipes.Application.Pagination;

namespace MomRecipes.Application.Recipes;

public sealed class RecipeSearchRequest : PageRequest
{
    public string? Name { get; init; }
    public int[] IngredientIds { get; init; } = [];
    public string[] IngredientNames { get; init; } = [];
    public int[] TagIds { get; init; } = [];
    public string[] TagNames { get; init; } = [];
    public int? MinTimeInMinutes { get; init; }
    public int? MaxTimeInMinutes { get; init; }
}
