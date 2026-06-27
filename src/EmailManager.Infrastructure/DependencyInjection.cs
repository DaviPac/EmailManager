using EmailManager.Application.Abstractions;
using EmailManager.Infrastructure.Persistence;
using EmailManager.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Default")));

        services.Configure<AttachmentStorageOptions>(o =>
            o.RootPath = config["AttachmentStorage:RootPath"] ?? "attachments");

        services.AddScoped<IAttachmentStorage, DiskAttachmentStorage>();
        return services;
    }
}