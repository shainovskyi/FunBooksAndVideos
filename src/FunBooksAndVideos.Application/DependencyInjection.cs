using FunBooksAndVideos.Application.Events;
using FunBooksAndVideos.Application.Rules;
using FunBooksAndVideos.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<PurchaseOrderCreated>, MembershipActivationRule>();
        services.AddScoped<IDomainEventHandler<PurchaseOrderCreated>, ShippingSlipRule>();

        services.AddScoped<PurchaseOrderProcessor>();
        services.AddScoped<PurchaseOrderService>();
        services.AddScoped<ProductService>();
        services.AddScoped<CustomerService>();

        return services;
    }
}
