namespace FunBooksAndVideos.Application.Events;

public interface IDomainEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default);
}
