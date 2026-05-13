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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using Stripe;


var builder = WebApplication.CreateBuilder(args);

// --- 1. KONTROLERI I JSON KONFIGURACIJA ---
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

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

// AutoMapper 15+: prvi argument je Action; skenira assembly gdje je MappingProfile.
// Opcionalno: AutoMapper:LicenseKey u konfiguraciji (vidi https://automapper.io).
builder.Services.AddAutoMapper(
    cfg =>
    {
        var lic = builder.Configuration["AutoMapper:LicenseKey"];
        if (!string.IsNullOrWhiteSpace(lic))
        {
            cfg.LicenseKey = lic;
        }
    },
    typeof(MappingProfile));

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

// --- DEV SEED LOGIN KORISNIKA (dummy hash iz migracija ne radi za login) ---
// U Development okruženju osiguraj radne test kredencijale:
// - admin / Admin123!
// - lana  / Lana123!
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<Korisnik>>();
        var roleManager = services.GetRequiredService<RoleManager<Uloga>>();

        // Uloge bi trebale već postojati iz seed-a, ali osiguraj ih za svaki slučaj.
        var rolesToEnsure = new[] { "Admin", "Klijent", "Zaposlenik" };
        foreach (var roleName in rolesToEnsure)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Uloga { Name = roleName, NormalizedName = roleName.ToUpperInvariant() });
            }
        }

        async Task EnsureUserAsync(
            string username,
            string email,
            string ime,
            string prezime,
            int gradId,
            string password,
            string roleName)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new Korisnik
                {
                    UserName = username,
                    Email = email,
                    Ime = ime,
                    Prezime = prezime,
                    GradId = gradId,
                    Status = true,
                    DatumRegistracije = DateTime.Now
                };

                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                {
                    var msg = string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new Exception($"Dev seed: CreateAsync({username}) failed: {msg}");
                }
            }

            // Force-set lozinke na poznatu vrijednost (radi čak i ako je seed ubacio dummy hash).
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, resetToken, password);
            if (!reset.Succeeded)
            {
                var msg = string.Join("; ", reset.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new Exception($"Dev seed: ResetPasswordAsync({username}) failed: {msg}");
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
            }
        }

        await EnsureUserAsync(
            username: "admin",
            email: "admin@nuaspa.ba",
            ime: "Admin",
            prezime: "NuaSpa",
            gradId: 1,
            password: "Admin123!",
            roleName: "Admin");

        await EnsureUserAsync(
            username: "lana",
            email: "lana@test.ba",
            ime: "Lana",
            prezime: "Korisnik",
            gradId: 3,
            password: "Lana123!",
            roleName: "Klijent");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Dev seed login korisnika nije uspio.");
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