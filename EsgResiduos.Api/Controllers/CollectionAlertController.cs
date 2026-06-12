// View somente leitura — alertas são gerados automaticamente em coletas e destinações.
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Controllers;

[ApiController]
[Route("api/collectionalerts")]
[Authorize]
public class CollectionAlertController(CollectionAlertViewModel collectionAlertViewModel) : ControllerBase
{
    private readonly CollectionAlertViewModel _collectionAlertViewModel = collectionAlertViewModel;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CollectionAlertResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Parâmetros de paginação inválidos. Use page >= 1 e pageSize entre 1 e 100.");
        }

        PagedResponse<CollectionAlertResponse> result = await _collectionAlertViewModel.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CollectionAlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        CollectionAlertResponse result = await _collectionAlertViewModel.GetByIdAsync(id);
        return Ok(result);
    }
}