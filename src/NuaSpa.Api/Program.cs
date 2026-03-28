using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NuaSpa.Infrastructure;
using System.Text.Json.Serialization;
// DODAJ OVO:
using NuaSpa.Application;

var builder = WebApplication.CreateBuilder(args);

// 1. Podrška za Controllere + IgnoreCycles
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// 2. Swagger konfiguracija
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NuaSpa API",
        Version = "v1",
        Description = "Backend API for Wellness & Spa System (Desktop & Mobile)"
    });

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
            new List<string>()
        }
    });
});

// 3. Registracija DbContext-a
builder.Services.AddDbContext<NuaSpaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. POPRAVLJENA Registracija AutoMapper-a 
// Koristimo typeof(MappingProfile) da bi AutoMapper znao da pretraži Application sloj
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

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

// BITNO: Authentication mora ići PRIJE Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();