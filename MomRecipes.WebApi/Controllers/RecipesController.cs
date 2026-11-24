using Microsoft.AspNetCore.Mvc;
using MomRecipes.Application.Pagination;
using MomRecipes.Application.Recipes;

namespace MomRecipes.WebApi.Controllers;

[ApiController]
[Route("api/recipes")]
public sealed class RecipesController(RecipeService recipeService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecipeDto>> Create(
        SaveRecipeRequest request, CancellationToken cancellationToken)
    {
        RecipeDto recipe;

        try
        {
            recipe = await recipeService.CreateAsync(request, cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return InvalidRecipe(exception);
        }

        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RecipeDto>> Update(
        Guid id, SaveRecipeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var recipe = await recipeService.UpdateAsync(id, request, cancellationToken);
            return recipe is null ? NotFound() : Ok(recipe);
        }
        catch (ArgumentException exception)
        {
            return InvalidRecipe(exception);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await recipeService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RecipeDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var recipe = await recipeService.GetByIdAsync(id, cancellationToken);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<RecipeDto>>> List(
        [FromQuery] RecipeSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await recipeService.SearchAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid recipe filters", detail: exception.Message);
        }
    }

    private ObjectResult InvalidRecipe(ArgumentException exception)
    {
        return Problem(statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid recipe", detail: exception.Message);
    }
}
