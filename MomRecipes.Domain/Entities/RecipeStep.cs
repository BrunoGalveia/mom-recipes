namespace MomRecipes.Domain.Entities;

public sealed class RecipeStep
{
    private RecipeStep()
    {
    }

    internal RecipeStep(Recipe recipe, int position, string instruction)
    {
        Recipe = recipe;
        RecipeId = recipe.Id;
        Position = position;
        UpdateInstruction(instruction);
    }

    public int Id { get; private set; }
    public Guid RecipeId { get; private set; }
    public Recipe Recipe { get; private set; } = null!;
    public int Position { get; private set; }
    public string Instruction { get; private set; } = null!;

    internal void UpdateInstruction(string instruction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instruction);
        Instruction = instruction.Trim();
    }
}
