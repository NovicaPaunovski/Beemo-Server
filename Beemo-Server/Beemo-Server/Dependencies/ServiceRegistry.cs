using Beemo_Server.Service.Implementations;
using Beemo_Server.Service.Interfaces;

namespace Beemo_Server.Dependencies
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
