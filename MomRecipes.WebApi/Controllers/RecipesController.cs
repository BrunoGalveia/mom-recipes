using Microsoft.AspNetCore.Mvc;
using MomRecipes.Application.Recipes;

namespace MomRecipes.WebApi.Controllers;

[ApiController]
[Route("api/recipes")]
public sealed class RecipesController(RecipeService recipeService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecipeDto>> Create(
        CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        RecipeDto recipe;

        try
        {
            recipe = await recipeService.CreateAsync(request, cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid recipe",
                Detail = exception.Message
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RecipeDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var recipe = await recipeService.GetByIdAsync(id, cancellationToken);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecipeDto>>> List(CancellationToken cancellationToken)
    {
        var recipes = await recipeService.ListAsync(cancellationToken);
        return Ok(recipes);
    }
}
