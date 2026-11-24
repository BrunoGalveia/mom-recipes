namespace MomRecipes.Application.Recipes;

public sealed record RecipeDto(
    Guid Id,
    string Title,
    string? Description,
    int? Servings,
    IReadOnlyList<RecipeIngredientDto> Ingredients,
    IReadOnlyList<RecipeStepDto> Steps);

public sealed record RecipeIngredientDto(string Name, decimal? Quantity, string? Unit);

public sealed record RecipeStepDto(int Position, string Instruction);
