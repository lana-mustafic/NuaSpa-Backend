using Microsoft.EntityFrameworkCore;
using NuaSpa.Infrastructure;
using NuaSpa.Worker;

var builder = Host.CreateApplicationBuilder(args);

// 1. Registracija pozadinskog servisa
builder.Services.AddHostedService<Worker>();

// 2. Registracija baze (samo jednom!)
builder.Services.AddDbContext<NuaSpaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Izgradnja i pokretanje
var host = builder.Build();
host.Run();