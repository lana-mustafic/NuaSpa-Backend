using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Application.Interfaces.Messaging;

namespace NuaSpa.Api.Controllers;

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
    public override async Task<UslugaDTO> Insert([FromBody] UslugaDTO dto)
    {
        var result = await base.Insert(dto);
        await _rabbitMQProducer.SendMessage(result, "usluge_queue");
        return result;
    }
}