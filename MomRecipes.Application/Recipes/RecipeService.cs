using MomRecipes.Domain.Entities;

namespace MomRecipes.Application.Recipes;

public sealed class RecipeService(IRecipeRepository repository)
{
    public async Task<RecipeDto> CreateAsync(CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        var recipe = new Recipe(request.Title, request.Description, request.Servings);

        foreach (var ingredient in request.Ingredients ?? [])
        {
            ArgumentNullException.ThrowIfNull(ingredient);
            recipe.AddIngredient(new Ingredient(ingredient.Name), ingredient.Quantity, ingredient.Unit);
        }

        foreach (var instruction in request.Steps ?? [])
        {
            recipe.AddStep(instruction);
        }

        await repository.AddAsync(recipe, cancellationToken);
        return ToDto(recipe);
    }

    public async Task<RecipeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var recipe = await repository.GetByIdAsync(id, cancellationToken);
        return recipe is null ? null : ToDto(recipe);
    }

    public async Task<IReadOnlyList<RecipeDto>> ListAsync(CancellationToken cancellationToken)
    {
        var recipes = await repository.ListAsync(cancellationToken);
        return recipes.Select(ToDto).ToArray();
    }

    private static RecipeDto ToDto(Recipe recipe) => new(
        recipe.Id,
        recipe.Title,
        recipe.Description,
        recipe.Servings,
        recipe.Ingredients.Select(ingredient => new RecipeIngredientDto(
            ingredient.Ingredient.Name, ingredient.Quantity, ingredient.Unit)).ToArray(),
        recipe.Steps.Select(step => new RecipeStepDto(step.Position, step.Instruction)).ToArray());
}
