using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NuaSpa.Api.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NuaSpa.Api.Data;
using NuaSpa.Application;
using NuaSpa.Application.Common;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces.Messaging;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using Stripe;
using NuaSpa.Application.Configuration;

EnvFileLoader.Load();
var builder = WebApplication.CreateBuilder(args);

// --- 1. KONTROLERI I JSON KONFIGURACIJA ---
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(typeof(NuaSpa.Application.MappingProfile).Assembly);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();

// --- CORS (eksplicitno dozvoljene originale) ---
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

allowedOrigins ??= new[]
{
    "http://localhost:3000",
    "http://127.0.0.1:3000",
    "http://localhost:8080",
    "http://127.0.0.1:8080"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("NuaSpaCors", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
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
        "Connection string 'DefaultConnection' nije postavljen. Kopirajte .env.example u .env i postavite ConnectionStrings__DefaultConnection.");
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
        "JwtSettings:Key mora biti postavljen (najmanje 32 znaka). Postavite JwtSettings__Key u .env datoteci.");
}

builder.Services.AddSingleton(jwtSettings);

var stripeSettings = new StripeSettings();
builder.Configuration.GetSection("Stripe").Bind(stripeSettings);
builder.Services.AddSingleton(stripeSettings);

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken)
                && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (string.IsNullOrWhiteSpace(jti))
            {
                return;
            }

            var revocation = context.HttpContext.RequestServices
                .GetRequiredService<NuaSpa.Application.Interfaces.ITokenRevocationService>();
            if (await revocation.IsRevokedAsync(jti, context.HttpContext.RequestAborted))
            {
                context.Fail("Token je opozvan (odjava).");
            }
        }
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
NuaSpa.Application.Configuration.ConfigurationValidator.RequireRabbitMq(builder.Configuration);
builder.Services.Configure<NuaSpa.Application.Messaging.RabbitMqOptions>(
    builder.Configuration.GetSection(NuaSpa.Application.Messaging.RabbitMqOptions.SectionName));
builder.Services.AddScoped<NuaSpa.Application.Interfaces.Messaging.IRabbitMqPublisher,
    NuaSpa.Application.Services.Messaging.RabbitMqPublisher>();
builder.Services.AddScoped<NuaSpa.Application.Interfaces.Messaging.INotificationPublisher,
    NuaSpa.Application.Services.Messaging.NotificationPublisher>();
builder.Services.AddScoped<NuaSpa.Application.Interfaces.IReportingService, NuaSpa.Application.Services.ReportingService>();
builder.Services.AddScoped<NuaSpa.Application.Services.SoftDeletePurgeService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<NuaSpa.Application.Interfaces.INotificationPushService,
    NuaSpa.Api.Services.SignalRNotificationPushService>();


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
        var logger = services.GetRequiredService<ILogger<Program>>();
        var pending = context.Database.GetPendingMigrations().ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation(
                "Primjenjujem {Count} migracija: {Migrations}",
                pending.Count,
                string.Join(", ", pending));
        }

        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Došlo je do greške prilikom migracije baze podataka.");
        throw;
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
        var rolesToEnsure = new[] { RoleConstants.Admin, RoleConstants.Klijent, RoleConstants.Zaposlenik };
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
                    throw new BusinessRuleException($"Dev seed: CreateAsync({username}) failed: {msg}");
                }
            }

            // Force-set lozinke na poznatu vrijednost (radi čak i ako je seed ubacio dummy hash).
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, resetToken, password);
            if (!reset.Succeeded)
            {
                var msg = string.Join("; ", reset.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new BusinessRuleException($"Dev seed: ResetPasswordAsync({username}) failed: {msg}");
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
            roleName: RoleConstants.Admin);

        await EnsureUserAsync(
            username: "lana",
            email: "lana@test.ba",
            ime: "Lana",
            prezime: "Korisnik",
            gradId: 3,
            password: "Lana123!",
            roleName: RoleConstants.Klijent);

        var context = services.GetRequiredService<NuaSpaContext>();
        var demoTherapist = await context.Zaposlenici
            .OrderBy(z => z.Id)
            .FirstOrDefaultAsync();
        if (demoTherapist == null)
        {
            demoTherapist = new Zaposlenik
            {
                Ime = "Thera",
                Prezime = "Pist",
                Specijalizacija = "Swedish Massage",
                DatumZaposlenja = DateTime.UtcNow,
                Status = ZaposlenikStatus.Active,
                CreatedAt = DateTime.UtcNow,
            };
            context.Zaposlenici.Add(demoTherapist);
            await context.SaveChangesAsync();
        }

        await EnsureUserAsync(
            username: "therapist",
            email: "therapist@nuaspa.ba",
            ime: "Thera",
            prezime: "Pist",
            gradId: 1,
            password: "Therapist123!",
            roleName: RoleConstants.Zaposlenik);

        var therapistUser = await userManager.FindByNameAsync("therapist");
        if (therapistUser != null && therapistUser.ZaposlenikId != demoTherapist.Id)
        {
            therapistUser.ZaposlenikId = demoTherapist.Id;
            await userManager.UpdateAsync(therapistUser);
        }
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

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DevelopmentDataSeeder.SeedAsync(
        scope.ServiceProvider,
        app.Environment,
        logger);
}

if (builder.Configuration.GetValue("UseHttpsRedirection", true))
{
    app.UseHttpsRedirection();
}

app.UseCors("NuaSpaCors");
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

var webRootPath = app.Environment.WebRootPath
    ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads", "usluge"));
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads", "obavijesti"));

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.Context.Request.Path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase))
        {
            if (!(ctx.Context.User.Identity?.IsAuthenticated ?? false))
            {
                ctx.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Context.Response.ContentLength = 0;
            }
        }
    }
});

app.MapControllers();
app.MapHub<NuaSpa.Api.Hubs.NotificationsHub>("/hubs/notifications");
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();