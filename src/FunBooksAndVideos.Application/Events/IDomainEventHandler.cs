namespace FunBooksAndVideos.Application.Events;

public interface IDomainEventHandler<in TEvent>
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
