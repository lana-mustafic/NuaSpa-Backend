using Microsoft.EntityFrameworkCore;
using NuaSpa.Domain;
using System.Text.Json.Serialization;
using NuaSpa.Application;
using System.Reflection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. KONTROLERI I JSON KONFIGURACIJA ---
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// --- 2. SWAGGER / OPENAPI (Task 1.1, 1.3) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NuaSpa API",
        Version = "v1",
        Description = "NuaSpa Wellness & Spa Management System API"
    });

    // JWT Security Definition
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Unesite JWT token u formatu: Bearer {vaš_token}"
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

    // XML dokumentacija (Task 1.3)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// --- 3. INFRASTRUKTURA (Baza i AutoMapper) ---
builder.Services.AddDbContext<NuaSpaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// --- 4. DEPENDENCY INJECTION (Task 1.4 - Senior Level Automation) ---
// Automatski registrujemo sve servise iz Application projekta
// Pravilo: Klasa mora završavati na "Service" i imati pripadajući interfejs (npr. KorisniciService -> IKorisniciService)
var applicationAssembly = typeof(MappingProfile).Assembly;

var serviceTypes = applicationAssembly.GetTypes()
    .Where(t => t.Name.EndsWith("Service") && !t.IsInterface && !t.IsAbstract);

foreach (var serviceType in serviceTypes)
{
    var interfaceType = serviceType.GetInterface($"I{serviceType.Name}");
    if (interfaceType != null)
    {
        builder.Services.AddScoped(interfaceType, serviceType);
    }
}

// --- 5. BUILD I MIDDLEWARE PIPELINE ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NuaSpa API v1");
        c.RoutePrefix = string.Empty; // Postavlja Swagger kao početnu stranicu (localhost:XXXX/)
    });
}

app.UseHttpsRedirection();

// Redoslijed je bitan: Authentication pa Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();