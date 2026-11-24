using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MomRecipes.Application.Catalog;
using MomRecipes.Application.Recipes;
using MomRecipes.Infrastructure.Catalog;
using MomRecipes.Infrastructure.Persistence;
using MomRecipes.Infrastructure.Recipes;

namespace MomRecipes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MomRecipesDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("MomRecipes");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Configure the 'MomRecipes' PostgreSQL connection string.");
            }

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        return services;
    }
}
