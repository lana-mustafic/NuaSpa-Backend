using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NuaSpa.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Podrška za Controllere + IgnoreCycles (sprječava beskonačne petlje u JSON-u)
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// 2. Swagger konfiguracija (US-4.1.3: Setup Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NuaSpa API",
        Version = "v1",
        Description = "Backend API for Wellness & Spa System (Desktop & Mobile)"
    });

    // Definisanje Security šeme (Bearer Token)
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Unesite token u formatu: Bearer {vaš_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    // FIX: Ispravna implementacija Security Requirementa za novije verzije
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>() // Koristimo List<string> umjesto string[] da izbjegnemo grešku
        }
    });
});

// 3. Registracija DbContext-a (Konekcija na SQL Server)
builder.Services.AddDbContext<NuaSpaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Registracija AutoMapper-a (Otkomentariši kada kreiraš MappingProfile)
// builder.Services.AddAutoMapper(typeof(Program)); 

var app = builder.Build();

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NuaSpa API v1");
    });
}

app.UseHttpsRedirection();

// Redoslijed je kritičan za sigurnost!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();