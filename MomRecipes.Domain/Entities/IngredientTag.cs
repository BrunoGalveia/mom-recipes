namespace MomRecipes.Domain.Entities;

public sealed class IngredientTag
{
    private IngredientTag()
    {
    }

    internal IngredientTag(Ingredient ingredient, Tag tag)
    {
        Ingredient = ingredient;
        IngredientId = ingredient.Id;
        Tag = tag;
        TagId = tag.Id;
    }

    public int IngredientId { get; private set; }
    public Ingredient Ingredient { get; private set; } = null!;
    public int TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;
}
