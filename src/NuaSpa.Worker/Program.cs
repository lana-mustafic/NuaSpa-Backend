using NuaSpa.Application.Messaging;
using NuaSpa.Worker;
using NuaSpa.Worker.Email;
using NuaSpa.Worker.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(SmtpOptions.SectionName));

builder.Services.AddSingleton<SmtpEmailSender>();
builder.Services.AddSingleton<FileOutboxEmailSender>();
builder.Services.AddSingleton<IEmailSender, CompositeEmailSender>();
builder.Services.AddSingleton<NotificationMessageDispatcher>();
builder.Services.AddHostedService<RabbitMqNotificationConsumer>();

var host = builder.Build();
host.Run();
