using Interkom.Core.Application.Interfaces;
using Interkom.Infrastructure.Persistence.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Interkom.Infrastructure.Persistence.Extensions
{
    public static class Registration
    {
        public static IServiceCollection AddPersistenceRegistration(this IServiceCollection services) 
        {
            services.AddScoped<IZenitelIntercomService, ZenitelInterkomService>();

            return services;
        }

    }
}
