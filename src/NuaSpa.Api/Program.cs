using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NuaSpa.Api.Services.Messaging;
using NuaSpa.Application;
using NuaSpa.Application.Common;
using NuaSpa.Application.Interfaces.Messaging;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using Stripe;


var builder = WebApplication.CreateBuilder(args);

// --- 1. KONTROLERI I JSON KONFIGURACIJA ---
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// --- 2. SWAGGER / OPENAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NuaSpa API",
        Version = "v1",
        Description = "NuaSpa Wellness & Spa Management System API"
    });

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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// --- 3. INFRASTRUKTURA (Baza, Identity i JWT) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' nije postavljen. Za Development: postavite u appsettings.Development.json ili " +
        "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"...\" / env ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<NuaSpaContext>(options =>
{
    options.UseSqlServer(connectionString, x => x.MigrationsAssembly("NuaSpa.Infrastructure"));
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddIdentity<Korisnik, Uloga>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<NuaSpaContext>()
.AddDefaultTokenProviders();

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:Key mora biti postavljen (najmanje 32 znaka). Za Development: appsettings.Development.json ili " +
        "dotnet user-secrets set \"JwtSettings:Key\" \"...\" / env JwtSettings__Key.");
}

builder.Services.AddSingleton(jwtSettings);

var stripeSettings = new StripeSettings();
builder.Configuration.GetSection("Stripe").Bind(stripeSettings);
builder.Services.AddSingleton(stripeSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = JwtRegisteredClaimNames.NameId
    };
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

// --- 4. DEPENDENCY INJECTION ---
builder.Services.AddScoped<IRabbitMQProducer, RabbitMQProducer>();
builder.Services.AddScoped<NuaSpa.Application.Interfaces.IReportingService, NuaSpa.Application.Services.ReportingService>();


var applicationAssembly = typeof(MappingProfile).Assembly;
var serviceTypes = applicationAssembly.GetTypes()
    .Where(t => t.Name.EndsWith("Service") && !t.IsInterface && !t.IsAbstract);

foreach (var serviceType in serviceTypes)
{
    // Ovo će naći IUslugaService za UslugaService
    var interfaceType = serviceType.GetInterface($"I{serviceType.Name}");
    if (interfaceType != null)
    {
        builder.Services.AddScoped(interfaceType, serviceType);
    }
}

var app = builder.Build();

// Stripe global configuration
if (!string.IsNullOrWhiteSpace(stripeSettings.SecretKey))
{
    StripeConfiguration.ApiKey = stripeSettings.SecretKey;
}

// --- AUTOMATSKE MIGRACIJE ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<NuaSpaContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Došlo je do greške prilikom migracije baze podataka.");
    }
}

// --- 5. MIDDLEWARE PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NuaSpa API v1");
        c.RoutePrefix = string.Empty;
    });
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NuaSpaContext>(); // Zamijeni 'NuaSpaContext' imenom tvog Context-a ako je drugačije
    if (!context.KategorijeUsluga.Any())
    {
        context.KategorijeUsluga.Add(new NuaSpa.Domain.Entities.KategorijaUsluga { Naziv = "Automatska", IsDeleted = false, CreatedAt = DateTime.Now });
        context.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();