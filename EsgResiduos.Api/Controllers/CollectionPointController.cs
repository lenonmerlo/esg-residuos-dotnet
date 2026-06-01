// GET /api/collectionpoints | GET /api/collectionpoints/{id} | POST /api/collectionpoints | PUT /api/collectionpoints/{id} | DELETE /api/collectionpoints/{id}
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Controllers;

[ApiController]
[Route("api/collectionpoints")]
[Authorize]
public class CollectionPointController(CollectionPointService collectionPointService) : ControllerBase
{
    private readonly CollectionPointService _collectionPointService = collectionPointService;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CollectionPointResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResponse<CollectionPointResponse> result = await _collectionPointService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CollectionPointResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        CollectionPointResponse result = await _collectionPointService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CollectionPointResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CollectionPointRequest request)
    {
        CollectionPointResponse result = await _collectionPointService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CollectionPointResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CollectionPointRequest request)
    {
        CollectionPointResponse result = await _collectionPointService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _collectionPointService.DeleteAsync(id);
        return NoContent();
    }
}