using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Movies.Infrastructure.Data;

namespace Movies.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MoviesDbContext>(options => options.UseSqlServer(connectionString));

        return services;
    }
}
