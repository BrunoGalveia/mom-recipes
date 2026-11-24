using Microsoft.EntityFrameworkCore;
using MomRecipes.Domain.Entities;

namespace MomRecipes.Infrastructure.Persistence;

public sealed class MomRecipesDbContext(DbContextOptions<MomRecipesDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();
    public DbSet<IngredientTag> IngredientTags => Set<IngredientTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(recipe =>
        {
            recipe.Property(entity => entity.Id).ValueGeneratedNever();

            recipe.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Recipes_Servings", "\"Servings\" > 0");
                table.HasCheckConstraint("CK_Recipes_TotalTimeInMinutes", "\"TotalTimeInMinutes\" >= 0");
            });

            recipe.Navigation(entity => entity.Ingredients).HasField("_ingredients")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            recipe.Navigation(entity => entity.Steps).HasField("_steps")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            recipe.Navigation(entity => entity.RecipeTags).HasField("_recipeTags")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<RecipeIngredient>(ingredient =>
        {
            ingredient.HasOne(entity => entity.Recipe)
                .WithMany(recipe => recipe.Ingredients)
                .HasForeignKey(entity => entity.RecipeId);
            ingredient.HasOne(entity => entity.Ingredient)
                .WithMany()
                .HasForeignKey(entity => entity.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
            ingredient.ToTable(table =>
                table.HasCheckConstraint("CK_RecipeIngredients_Quantity", "\"Quantity\" > 0"));
        });

        modelBuilder.Entity<RecipeStep>(step =>
        {
            step.HasOne(entity => entity.Recipe)
                .WithMany(recipe => recipe.Steps)
                .HasForeignKey(entity => entity.RecipeId);
            step.HasIndex(entity => new { entity.RecipeId, entity.Position }).IsUnique();
            step.ToTable(table =>
                table.HasCheckConstraint("CK_RecipeSteps_Position", "\"Position\" > 0"));
        });

        modelBuilder.Entity<Tag>().HasIndex(entity => entity.Name);

        modelBuilder.Entity<RecipeTag>(tag =>
        {
            tag.HasKey(entity => new { entity.RecipeId, entity.TagId });
            tag.HasIndex(entity => new { entity.TagId, entity.RecipeId });
            tag.HasOne(entity => entity.Recipe)
                .WithMany(recipe => recipe.RecipeTags)
                .HasForeignKey(entity => entity.RecipeId);
            tag.HasOne(entity => entity.Tag)
                .WithMany()
                .HasForeignKey(entity => entity.TagId);
        });

        modelBuilder.Entity<IngredientTag>(tag =>
        {
            tag.HasKey(entity => new { entity.IngredientId, entity.TagId });
            tag.HasIndex(entity => new { entity.TagId, entity.IngredientId });
            tag.HasOne(entity => entity.Ingredient)
                .WithMany(ingredient => ingredient.IngredientTags)
                .HasForeignKey(entity => entity.IngredientId);
            tag.HasOne(entity => entity.Tag)
                .WithMany()
                .HasForeignKey(entity => entity.TagId);
        });

        modelBuilder.Entity<Ingredient>().Navigation(entity => entity.IngredientTags).HasField("_ingredientTags")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
