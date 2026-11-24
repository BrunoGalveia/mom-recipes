namespace MomRecipes.Application.Recipes;

public sealed record CreateRecipeRequest(
    string Title,
    string? Description,
    int? Servings,
    IReadOnlyList<CreateRecipeIngredientRequest?>? Ingredients,
    IReadOnlyList<string>? Steps);

public sealed record CreateRecipeIngredientRequest(string Name, decimal? Quantity, string? Unit);
