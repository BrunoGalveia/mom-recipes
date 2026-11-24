namespace MomRecipes.Domain.Entities;

public sealed class Recipe
{
    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<RecipeStep> _steps = [];
    private readonly List<RecipeTag> _recipeTags = [];

    private Recipe()
    {
        Ingredients = _ingredients.AsReadOnly();
        Steps = _steps.AsReadOnly();
        RecipeTags = _recipeTags.AsReadOnly();
    }

    public Recipe(
        string title,
        string? description = null,
        int? servings = null,
        string? imageUrl = null,
        int? totalTimeInMinutes = null) : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (servings is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(servings), "Servings must be greater than zero.");
        }

        if (totalTimeInMinutes is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalTimeInMinutes), "Total time cannot be negative.");
        }

        Id = Guid.NewGuid();
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        Servings = servings;
        TotalTimeInMinutes = totalTimeInMinutes;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public int? Servings { get; private set; }
    public int? TotalTimeInMinutes { get; private set; }
    public IReadOnlyList<RecipeIngredient> Ingredients { get; }
    public IReadOnlyList<RecipeStep> Steps { get; }
    public IReadOnlyList<RecipeTag> RecipeTags { get; }

    public void ReplaceContents(Recipe replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        if (ReferenceEquals(this, replacement))
        {
            return;
        }

        Title = replacement.Title;
        Description = replacement.Description;
        ImageUrl = replacement.ImageUrl;
        Servings = replacement.Servings;
        TotalTimeInMinutes = replacement.TotalTimeInMinutes;

        _ingredients.Clear();
        foreach (var ingredient in replacement.Ingredients)
        {
            AddIngredient(ingredient.Ingredient, ingredient.Quantity, ingredient.Unit, ingredient.Notes);
        }

        // Reuse existing rows by position so the unique index stays valid throughout an edit.
        var stepsByPosition = _steps.ToDictionary(step => step.Position);
        _steps.RemoveAll(step => step.Position > replacement.Steps.Count);
        foreach (var step in replacement.Steps)
        {
            if (stepsByPosition.TryGetValue(step.Position, out var existing))
            {
                existing.UpdateInstruction(step.Instruction);
            }
            else
            {
                _steps.Add(new RecipeStep(this, step.Position, step.Instruction));
            }
        }
        _steps.Sort((first, second) => first.Position.CompareTo(second.Position));

        var retainedTagIds = replacement.RecipeTags.Select(link => link.TagId).Where(id => id != 0).ToHashSet();
        _recipeTags.RemoveAll(link => !retainedTagIds.Contains(link.TagId));
        foreach (var tag in replacement.RecipeTags)
        {
            AddTag(tag.Tag);
        }
    }

    public void AddIngredient(
        Ingredient ingredient,
        decimal? quantity = null,
        string? unit = null,
        string? notes = null)
    {
        _ingredients.Add(new RecipeIngredient(this, ingredient, quantity, unit, notes));
    }

    public void AddStep(string instruction)
    {
        _steps.Add(new RecipeStep(this, _steps.Count + 1, instruction));
    }

    public void AddTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_recipeTags.Any(link => link.Tag == tag || (tag.Id != 0 && link.TagId == tag.Id)))
        {
            return;
        }

        _recipeTags.Add(new RecipeTag(this, tag));
    }
}
