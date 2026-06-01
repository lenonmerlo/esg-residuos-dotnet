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

namespace EsgResiduos.Api.Services;

public class AuthService(AppDbContext context, IConfiguration config)
{
    private readonly AppDbContext _context = context;
    private readonly IConfiguration _config = config;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        DbSet<User> users = _context.Users ?? throw new InvalidOperationException("Users DbSet is not configured.");

        if (await users.AnyAsync(u => u.Email == request.Email))
        {
            throw new AppException("Email already in use.", 409);
        }

        User user = new()
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _ = users.Add(user);
        _ = await _context.SaveChangesAsync();

        return GenerateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        DbSet<User> users = _context.Users ?? throw new InvalidOperationException("Users DbSet is not configured.");

        User user = await users.FirstOrDefaultAsync(u => u.Email == request.Email)
            ?? throw new AppException("Invalid credentials.", 401);

        return !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
            ? throw new AppException("Invalid credentials.", 401)
            : GenerateToken(user);
    }

    private AuthResponse GenerateToken(User user)
    {
        string expirationHoursValue = _config["Jwt:ExpirationHours"]
            ?? throw new AppException("JWT expiration is not configured.", 500);

        if (!int.TryParse(expirationHoursValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int expirationHours))
        {
            throw new AppException("JWT expiration must be a valid integer.", 500);
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