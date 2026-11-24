namespace MomRecipes.Domain.Entities;

public sealed class RecipeIngredient
{
    private RecipeIngredient()
    {
    }

    internal RecipeIngredient(Recipe recipe, Ingredient ingredient, decimal? quantity, string? unit, string? notes)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (quantity is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        Recipe = recipe;
        RecipeId = recipe.Id;
        Ingredient = ingredient;
        IngredientId = ingredient.Id;
        Quantity = quantity;
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public int Id { get; private set; }
    public Guid RecipeId { get; private set; }
    public Recipe Recipe { get; private set; } = null!;
    public int IngredientId { get; private set; }
    public Ingredient Ingredient { get; private set; } = null!;
    public decimal? Quantity { get; private set; }
    public string? Unit { get; private set; }
    public string? Notes { get; private set; }
}
