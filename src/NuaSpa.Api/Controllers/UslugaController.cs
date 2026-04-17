using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Application.Interfaces.Messaging;

namespace NuaSpa.Api.Controllers;

public class UslugaController : BaseController<UslugaDTO, UslugaSearchObject>
{
    private readonly IRabbitMQProducer _rabbitMQProducer;

    public UslugaController(
        IBaseService<UslugaDTO, UslugaSearchObject> service,
        IRabbitMQProducer rabbitMQProducer) : base(service)
    {
        _rabbitMQProducer = rabbitMQProducer;
    }

    [HttpPost]
    public override async Task<UslugaDTO> Insert([FromBody] UslugaDTO dto)
    {
        // 1. Pozivamo bazu (BaseService)
        var result = await base.Insert(dto);

        // 2. Šaljemo poruku u RabbitMQ
        _rabbitMQProducer.SendMessage(result, "usluge_queue");

        return result;
    }
}