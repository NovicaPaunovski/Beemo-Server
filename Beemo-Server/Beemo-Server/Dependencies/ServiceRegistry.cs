using Beemo_Server.Data.Repositories.Implementations;
using Beemo_Server.Data.Repositories.Interfaces;
using Beemo_Server.Service.Implementations;
using Beemo_Server.Service.Interfaces;

namespace Beemo_Server.Dependencies
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            /* Services */
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();

            /* Repositories */
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
