namespace MomRecipes.Domain.Entities;

public sealed class Ingredient
{
    private readonly List<IngredientTag> _ingredientTags = [];

    private Ingredient()
    {
        IngredientTags = _ingredientTags.AsReadOnly();
    }

    public Ingredient(string name, string? imageUrl = null) : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
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
