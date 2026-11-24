namespace MomRecipes.Domain.Entities;

public sealed class Tag
{
    public Tag(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public int Id { get; private set; }
    public string Name { get; }
}
