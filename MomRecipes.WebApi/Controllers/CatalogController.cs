using Microsoft.AspNetCore.Mvc;
using MomRecipes.Application.Catalog;
using MomRecipes.Application.Pagination;

namespace MomRecipes.WebApi.Controllers;

[ApiController]
[Route("api")]
public sealed class CatalogController(CatalogService catalogService) : ControllerBase
{
    [HttpGet("ingredients")]
    public Task<ActionResult<PagedResult<IngredientDto>>> ListIngredients(
        [FromQuery] PageRequest request, CancellationToken cancellationToken)
    {
        return ReadPageAsync(() => catalogService.ListIngredientsAsync(request, cancellationToken));
    }

    [HttpGet("tags")]
    public Task<ActionResult<PagedResult<TagDto>>> ListTags(
        [FromQuery] PageRequest request, CancellationToken cancellationToken)
    {
        return ReadPageAsync(() => catalogService.ListTagsAsync(request, cancellationToken));
    }

    private async Task<ActionResult<PagedResult<T>>> ReadPageAsync<T>(Func<Task<PagedResult<T>>> read)
    {
        try
        {
            return Ok(await read());
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid pagination", detail: exception.Message);
        }
    }
}
