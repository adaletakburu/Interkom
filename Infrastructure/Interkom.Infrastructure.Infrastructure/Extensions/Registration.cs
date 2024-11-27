using Interkom.Core.Application.Interfaces;
using Interkom.Infrastructure.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Interkom.Infrastructure.Infrastructure.Extensions
{
    public static class Registration
    {
        public static IServiceCollection AddInfrastructureRegistration(this IServiceCollection services) 
        {
            services.AddScoped<IZenitelIntercomService, ZenitelInterkomService>();

            return services;
        }

    }
}
