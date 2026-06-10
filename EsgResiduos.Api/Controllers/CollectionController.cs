// View (MVVM): endpoints de coleta. Volume e alertas automáticos ficam no CollectionViewModel.
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Controllers;

[ApiController]
[Route("api/collections")]
[Authorize]
public class CollectionController(CollectionViewModel collectionViewModel) : ControllerBase
{
    private readonly CollectionViewModel _collectionViewModel = collectionViewModel;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CollectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResponse<CollectionResponse> result = await _collectionViewModel.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CollectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        CollectionResponse result = await _collectionViewModel.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CollectionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CollectionRequest request)
    {
        CollectionResponse result = await _collectionViewModel.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _collectionViewModel.DeleteAsync(id);
        return NoContent();
    }
}