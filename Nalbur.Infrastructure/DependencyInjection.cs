using Microsoft.Extensions.DependencyInjection;
using Nalbur.Domain.Interfaces;
using Nalbur.Infrastructure.Data;
using Nalbur.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Nalbur.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NalburDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IInstallmentService, InstallmentService>();
        services.AddScoped<IReminderService, ReminderService>();

        return services;
    }
}
