using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Infrastructure.Persistence;
using FunBooksAndVideos.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
        services.AddSingleton(new DatabaseInitializer(connectionString));

        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IShippingSlipRepository, ShippingSlipRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();

        return services;
    }
}
