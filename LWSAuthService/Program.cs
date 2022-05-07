using LWSAuthService.Configuration;
using LWSAuthService.Repository;
using LWSAuthService.Service;
using LWSAuthService.Service.Consumers;
using LWSEvent.Event.Account;
using MassTransit;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var rabbitMqSection = builder.Configuration.GetSection("RabbitMqSection")
    .Get<RabbitMqConfiguration>();
builder.Services.AddMassTransit(a =>
{
    a.AddConsumer<TokenCreationConsumer>();
    a.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbitMqSection.Host, rabbitMqSection.VirtualHost, h =>
        {
            h.Username(rabbitMqSection.UserName);
            h.Password(rabbitMqSection.Password);
        });

        cfg.Message<AccountCreatedEvent>(x =>
        {
            // This is the 'topic' in ASB, or 'exchange' in RabbitMQ
            x.SetEntityName("account.created");
        });

        cfg.Message<AccountDeletedEvent>(x =>
        {
            // This is the 'topic' in ASB, or 'exchange' in RabbitMQ
            x.SetEntityName("account.deleted");
        });
        
        cfg.ReceiveEndpoint("token.created:AuthConsumer", config =>
        {
            config.Bind("token.created");
            config.ConfigureConsumer<TokenCreationConsumer>(ctx);
        });
    });
});

var targetSection = builder.Configuration.GetSection("MongoSection").Get<MongoConfiguration>();
builder.Services.AddSingleton(targetSection);
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Scoped Service(Mostly Business Logic)
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<AccessTokenService>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

// Add Singleton Service(Mostly Data Logic)
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<IAccessTokenRepository, AccessTokenRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMetricServer();
app.UseHttpMetrics();

app.UseAuthorization();

app.MapControllers();

app.Run();