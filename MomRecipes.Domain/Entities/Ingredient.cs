namespace MomRecipes.Domain.Entities;

public sealed class Ingredient
{
    private readonly List<IngredientTag> _ingredientTags = [];

    public Ingredient(string name, string? imageUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        IngredientTags = _ingredientTags.AsReadOnly();
    }

    public int Id { get; private set; }
    public string Name { get; }
    public string? ImageUrl { get; }
    public IReadOnlyList<IngredientTag> IngredientTags { get; }

    public void AddTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_ingredientTags.Any(link => link.Tag == tag || (tag.Id != 0 && link.TagId == tag.Id)))
        {
            return;
        }

        _ingredientTags.Add(new IngredientTag(this, tag));
    }
}
