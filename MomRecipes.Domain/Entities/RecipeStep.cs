namespace MomRecipes.Domain.Entities;

public sealed class RecipeStep
{
    internal RecipeStep(Recipe recipe, int position, string instruction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instruction);

        Recipe = recipe;
        RecipeId = recipe.Id;
        Position = position;
        Instruction = instruction.Trim();
    }

    public int Id { get; private set; }
    public Guid RecipeId { get; private set; }
    public Recipe Recipe { get; private set; }
    public int Position { get; }
    public string Instruction { get; }
}
