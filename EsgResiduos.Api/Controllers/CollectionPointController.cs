// View (MVVM): CRUD de pontos de coleta com paginação na listagem.
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Controllers;

[ApiController]
[Route("api/collectionpoints")]
[Authorize]
public class CollectionPointController(CollectionPointViewModel collectionPointViewModel) : ControllerBase
{
    private readonly CollectionPointViewModel _collectionPointViewModel = collectionPointViewModel;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CollectionPointResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResponse<CollectionPointResponse> result = await _collectionPointViewModel.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CollectionPointResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        CollectionPointResponse result = await _collectionPointViewModel.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CollectionPointResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CollectionPointRequest request)
    {
        CollectionPointResponse result = await _collectionPointViewModel.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CollectionPointResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CollectionPointRequest request)
    {
        CollectionPointResponse result = await _collectionPointViewModel.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _collectionPointViewModel.DeleteAsync(id);
        return NoContent();
    }
}