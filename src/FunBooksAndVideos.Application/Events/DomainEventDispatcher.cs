using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Application.Events;

public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
    {
        foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TEvent>>())
        {
            await handler.HandleAsync(domainEvent, cancellationToken);
        }
    }
}
