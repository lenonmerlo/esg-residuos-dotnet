// View (MVVM): registra destinação e delega a notificação de descarte ao DestinationViewModel.
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Controllers;

[ApiController]
[Route("api/destinations")]
[Authorize]
public class DestinationController(DestinationViewModel destinationViewModel) : ControllerBase
{
    private readonly DestinationViewModel _destinationViewModel = destinationViewModel;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DestinationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResponse<DestinationResponse> result = await _destinationViewModel.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DestinationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        DestinationResponse result = await _destinationViewModel.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DestinationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] DestinationRequest request)
    {
        DestinationResponse result = await _destinationViewModel.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}