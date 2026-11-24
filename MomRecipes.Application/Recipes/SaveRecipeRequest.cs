namespace MomRecipes.Application.Recipes;

public sealed record SaveRecipeRequest(
    string Title,
    string? Description,
    int? Servings,
    IReadOnlyList<RecipeIngredientRequest?>? Ingredients,
    IReadOnlyList<string>? Steps,
    string? ImageUrl = null,
    int? TotalTimeInMinutes = null,
    IReadOnlyList<int>? TagIds = null,
    IReadOnlyList<string>? TagNames = null);

public sealed record RecipeIngredientRequest(
    string? Name,
    decimal? Quantity,
    string? Unit,
    int? IngredientId = null,
    string? Notes = null);
