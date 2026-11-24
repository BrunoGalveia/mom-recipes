using MomRecipes.Application.Catalog;

namespace MomRecipes.Application.Recipes;

public sealed record RecipeDto(
    Guid Id,
    string Title,
    string? Description,
    int? Servings,
    IReadOnlyList<RecipeIngredientDto> Ingredients,
    IReadOnlyList<RecipeStepDto> Steps,
    string? ImageUrl,
    int? TotalTimeInMinutes,
    IReadOnlyList<TagDto> Tags);

public sealed record RecipeIngredientDto(
    string Name, decimal? Quantity, string? Unit, int IngredientId, string? Notes);

public sealed record RecipeStepDto(int Position, string Instruction);
