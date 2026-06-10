using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using EsgResiduos.Api.Data;
using EsgResiduos.Api.DTOs.Request;
using EsgResiduos.Api.DTOs.Response;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EsgResiduos.Api.ViewModels;

public class AuthViewModel(AppDbContext context, IConfiguration config)
{
    private readonly AppDbContext _context = context;
    private readonly IConfiguration _config = config;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new AppException("E-mail já cadastrado.", 409);
        }

        User user = new()
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _ = _context.Users.Add(user);
        _ = await _context.SaveChangesAsync();

        return GenerateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        return user is not null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
            ? GenerateToken(user)
            : throw new AppException("Credenciais inválidas.", 401);
    }

    private AuthResponse GenerateToken(User user)
    {
        string expirationHoursValue = _config["Jwt:ExpirationHours"]
            ?? throw new AppException("Expiração do JWT não configurada.", 500);

        if (!int.TryParse(expirationHoursValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int expirationHours))
        {
            throw new AppException("Expiração do JWT deve ser um número inteiro.", 500);
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
        DateTime expiration = DateTime.UtcNow.AddHours(expirationHours);

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        ];

        JwtSecurityToken token = new(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiration
        };
    }
}
