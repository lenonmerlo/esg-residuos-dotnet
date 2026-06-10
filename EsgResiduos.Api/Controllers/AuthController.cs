// Autenticação JWT — único ponto público além do Swagger; protege os demais endpoints.
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.ViewModels;

using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController(AuthViewModel authViewModel) : ControllerBase
{
    private readonly AuthViewModel _authViewModel = authViewModel;

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        AuthResponse response = await _authViewModel.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        AuthResponse response = await _authViewModel.LoginAsync(request);
        return Ok(response);
    }
}