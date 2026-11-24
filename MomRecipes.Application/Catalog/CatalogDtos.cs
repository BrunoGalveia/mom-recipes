namespace MomRecipes.Application.Catalog;

public sealed record IngredientDto(int Id, string Name, string? ImageUrl);

public sealed record TagDto(int Id, string Name);
