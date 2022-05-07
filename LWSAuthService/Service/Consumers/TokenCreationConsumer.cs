using LWSEvent.Event.Account;
using MassTransit;
using Newtonsoft.Json;

namespace LWSAuthService.Service.Consumers;

public class TokenCreationConsumer: IConsumer<TokenCreatedEvent>
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public TokenCreationConsumer(ILogger<TokenCreationConsumer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public async Task Consume(ConsumeContext<TokenCreatedEvent> context)
    {
        _logger.LogInformation("Consumed Message, Message Content: {MessageContent}", JsonConvert.SerializeObject(context.Message));
        await using var scope = _serviceProvider.CreateAsyncScope();
        var accountService = scope.ServiceProvider.GetService<AccountService>()!;

        // Set Namespace Token
        await accountService.SetNamespaceToken(context.Message);
    }
}