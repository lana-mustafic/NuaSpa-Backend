using Microsoft.EntityFrameworkCore;
 // Za OpenApiInfo grešku
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Infrastructure; // POPRAVLJENO: Izbačeno .Database jer je tvoj Context ovdje
//using NuaSpa.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Dodaj podršku za Controllere
builder.Services.AddControllers();

// 2. Swagger konfiguracija (Ovo će ti trebati za Task 2.5)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
   // c.SwaggerDoc("v1", new OpenApiInfo { Title = "NuaSpa API", Version = "v1" });
});

// 3. Registracija DbContext-a (Konekcija na SQL Server)
builder.Services.AddDbContext<NuaSpaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Registracija AutoMapper-a (Koristi tvoj MappingProfile)
builder.Services.AddAutoMapper(typeof(NuaSpa.Application.MappingProfile));

// 5. Registracija Servisa (Dependency Injection)
// Ovdje mapiramo Interfejs na konkretnu klasu Servisa
//builder.Services.AddScoped<IBaseService<UslugaDTO, UslugaSearchObject>, UslugaService>();

var app = builder.Build();

// 6. Konfiguracija HTTP pipeline-a
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// 7. Mapiranje ruta controllera
app.MapControllers();

app.Run();