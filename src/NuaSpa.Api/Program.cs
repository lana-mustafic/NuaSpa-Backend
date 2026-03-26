using Microsoft.EntityFrameworkCore;
using NuaSpa.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Dodaj podršku za Controllere (neophodno za API)
builder.Services.AddControllers();

// 2. Dodaj Swagger (za testiranje API-ja)
builder.Services.AddEndpointsApiExplorer();


// 3. Registracija DbContext-a (Povezivanje sa tvojom bazom 180081)
builder.Services.AddDbContext<NuaSpaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();



app.UseAuthorization();

// 5. Mapiranje ruta controllera
app.MapControllers();

app.Run();