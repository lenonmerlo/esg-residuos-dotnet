// GET /api/collectionalerts | GET /api/collectionalerts/{id}
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Controllers;

[ApiController]
[Route("api/collectionalerts")]
[Authorize]
public class CollectionAlertController(CollectionAlertService collectionAlertService) : ControllerBase
{
    private readonly CollectionAlertService _collectionAlertService = collectionAlertService;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CollectionAlertResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResponse<CollectionAlertResponse> result = await _collectionAlertService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CollectionAlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        CollectionAlertResponse result = await _collectionAlertService.GetByIdAsync(id);
        return Ok(result);
    }
}