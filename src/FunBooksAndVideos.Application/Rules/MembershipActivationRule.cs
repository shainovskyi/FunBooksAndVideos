using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Application.Events;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Rules;

/// <summary>BR1: memberships in the order are activated on the customer account immediately.</summary>
public class MembershipActivationRule(ICustomerRepository customerRepository) : IDomainEventHandler<PurchaseOrderCreated>
{
    public async Task HandleAsync(PurchaseOrderCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var order = domainEvent.Order;
        if (!order.ContainsMemberships)
            return;

        var customer = await customerRepository.GetByIdAsync(order.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), order.CustomerId);

        var changed = false;
        foreach (var membership in order.GetMemberships())
        {
            changed |= customer.ActivateMembership(membership);
        }

        if (changed)
        {
            await customerRepository.UpdateMembershipsAsync(customer, cancellationToken);
        }
    }
}
