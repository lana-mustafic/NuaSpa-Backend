using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Application.Interfaces.Messaging;

namespace NuaSpa.Api.Controllers;

[Authorize]
public class UslugaController : BaseController<UslugaDTO, UslugaSearchObject>
{
    private readonly IRabbitMQProducer _rabbitMQProducer;
    // Promijenjeno: sada koristimo IUslugaService
    private readonly IUslugaService _uslugaService;

    public UslugaController(
        IUslugaService service,
        IRabbitMQProducer rabbitMQProducer) : base(service)
    {
        _uslugaService = service;
        _rabbitMQProducer = rabbitMQProducer;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public override async Task<UslugaDTO> Insert([FromBody] UslugaDTO dto)
    {
        var result = await base.Insert(dto);
        await _rabbitMQProducer.SendMessage(result, "usluge_queue");
        return result;
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UslugaDTO>> Update(int id, [FromBody] UslugaDTO dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("ID u ruti i u tijelu zahtjeva se ne poklapaju.");
        }

        try
        {
            var updated = await _uslugaService.UpdateAsync(dto);
            await _rabbitMQProducer.SendMessage(updated, "usluge_queue");
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, message) = await _uslugaService.DeleteAsync(id);
        if (!ok)
        {
            return Conflict(new { message });
        }

        return NoContent();
    }
}