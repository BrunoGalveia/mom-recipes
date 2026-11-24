namespace MomRecipes.Domain.Entities;

public sealed class IngredientTag
{
    internal IngredientTag(Ingredient ingredient, Tag tag)
    {
        Ingredient = ingredient;
        IngredientId = ingredient.Id;
        Tag = tag;
        TagId = tag.Id;
    }

    public int IngredientId { get; private set; }
    public Ingredient Ingredient { get; private set; }
    public int TagId { get; private set; }
    public Tag Tag { get; private set; }
}
