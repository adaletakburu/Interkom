using Interkom.Core.Application.Interfaces;
using Interkom.Infrastructure.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Stentofon.AlphaCom.AlphaNet.Client;

namespace Interkom.Infrastructure.Infrastructure.Extensions
{
    public static class Registration
    {
        public static IServiceCollection AddInfrastructureRegistration(this IServiceCollection services) 
        {
            services.AddScoped<IZenitelIntercomService, ZenitelInterkomService>();

            services.AddSingleton<AlphaNetClient>(provider =>
            {
                var client = new AlphaNetClient("10.0.1.15");
                client.Connect();

                // Bağlantı kurulana kadar bekleme (async uygun değil, dikkat!)
                while (!client.IsConnected)
                {
                    Console.WriteLine("Bağlantı bekleniyor.");
                    Thread.Sleep(1000); // Daha iyi bir çözüm için asenkron hale getirilebilir.
                }
                Task.Delay(10000).Wait();
                Console.WriteLine("Bağlantı kuruldu.");
                return client;
            });

            return services;
        }

    }
}
