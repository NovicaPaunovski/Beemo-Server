﻿using Beemo_Server.Data.Repositories.Implementations;
using Beemo_Server.Data.Repositories.Interfaces;
using Beemo_Server.Service.Implementations;
using Beemo_Server.Service.Interfaces;

namespace Beemo_Server.Dependencies
{
    public static class ServiceRegistry
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);

            /* Services */
            services.AddScoped<IUserService, UserService>();

            /* Repositories */
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
