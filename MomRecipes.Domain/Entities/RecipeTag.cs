namespace MomRecipes.Domain.Entities;

public sealed class RecipeTag
{
    internal RecipeTag(Recipe recipe, Tag tag)
    {
        Recipe = recipe;
        RecipeId = recipe.Id;
        Tag = tag;
        TagId = tag.Id;
    }

    public Guid RecipeId { get; private set; }
    public Recipe Recipe { get; private set; }
    public int TagId { get; private set; }
    public Tag Tag { get; private set; }
}
