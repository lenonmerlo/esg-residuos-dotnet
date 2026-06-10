using System.Text;

using EsgResiduos.Api.Data;
using EsgResiduos.Api.Exceptions;
using EsgResiduos.Api.ViewModels;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Em testes (xUnit) usamos banco em memória; em execução normal, SQL Server via connection string.
bool isTestHost = AppDomain.CurrentDomain.FriendlyName.Contains("testhost", StringComparison.OrdinalIgnoreCase);

Action<DbContextOptionsBuilder> configureDbContext = isTestHost
    ? options => options.UseInMemoryDatabase("EsgResiduosTestsDb")
    : options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

_ = builder.Services.AddDbContext<AppDbContext>(configureDbContext);

// JWT Auth
string jwtKey = builder.Configuration["Jwt:Key"]!;
string jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
string jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    });
// MVVM — ViewModels concentram a lógica de negócio; Controllers ficam finos (só HTTP).
builder.Services.AddScoped<AuthViewModel>();
builder.Services.AddScoped<WasteTypeViewModel>();
builder.Services.AddScoped<CollectionPointViewModel>();
builder.Services.AddScoped<CollectionViewModel>();
builder.Services.AddScoped<DestinationViewModel>();
builder.Services.AddScoped<CollectionAlertViewModel>();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();



// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ESG Resíduos API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    _ = app.Lifetime.ApplicationStarted.Register(() =>
    {
        IServer server = app.Services.GetRequiredService<IServer>();
        IServerAddressesFeature? addresses = server.Features.Get<IServerAddressesFeature>();

        if (addresses?.Addresses is null || addresses.Addresses.Count == 0)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("  ESG Resíduos API — documentação interativa:");
        foreach (string address in addresses.Addresses)
        {
            Console.WriteLine($"  → {address.TrimEnd('/')}/swagger");
        }
        Console.WriteLine();
    });
}

app.Run();

// Necessário para testes de integração
public partial class Program { }