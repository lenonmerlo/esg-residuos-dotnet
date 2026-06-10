// View (MVVM): categorias de resíduo usadas no lançamento de coletas.
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Controllers;

[ApiController]
[Route("api/wastetypes")]
[Authorize]
public class WasteTypeController(WasteTypeViewModel wasteTypeViewModel) : ControllerBase
{
    private readonly WasteTypeViewModel _wasteTypeViewModel = wasteTypeViewModel;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<WasteTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResponse<WasteTypeResponse> result = await _wasteTypeViewModel.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WasteTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        WasteTypeResponse result = await _wasteTypeViewModel.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WasteTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] WasteTypeRequest request)
    {
        WasteTypeResponse result = await _wasteTypeViewModel.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WasteTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] WasteTypeRequest request)
    {
        WasteTypeResponse result = await _wasteTypeViewModel.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _wasteTypeViewModel.DeleteAsync(id);
        return NoContent();
    }


}