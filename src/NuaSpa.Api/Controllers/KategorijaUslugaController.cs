using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.DTOs; // Provjeri tačan namespace za tvoj DTO

namespace NuaSpa.Api.Controllers
{
    public class KategorijaUslugaController : BaseController<KategorijaUslugaDTO, object>
    {
        public KategorijaUslugaController(IKategorijaUslugaService service) : base(service)
        {
        }
    }
}