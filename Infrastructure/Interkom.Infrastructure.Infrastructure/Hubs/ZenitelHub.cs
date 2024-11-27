using Microsoft.AspNetCore.SignalR;
using Stentofon.AlphaCom.AlphaNet.Client;
using Stentofon.AlphaCom.AlphaNet.Messages;

namespace Interkom.Infrastructure.Infrastructure.Hubs
{
    public class ZenitelHub : Hub
    {
        private readonly AlphaNetClient _client = new AlphaNetClient("10.0.1.15");
        
        private readonly IHubContext<ZenitelHub> hubContext;

        public ZenitelHub(IHubContext<ZenitelHub> context)
        {
            hubContext = context;
        }

        public override async Task OnConnectedAsync()//BAĞLANTI SAĞLANDIĞINDA
        {
            _client.Connect();
            while (!_client.IsConnected)
            {
                Console.WriteLine("Bağlantı bekleniyor.");
                Task.Delay(1000).Wait();
            }
            Console.WriteLine("Bağlandı ve veriler bekleniyor.");
            Task.Delay(10000).Wait();

            await hubContext.Clients.All.SendAsync("GetFullIPStationOKList", _client.Stations.GetFullStationList().Where(item => item.IsIPStationOK()));
            _client.OnStationState += HandleOnStationState;
            
            await base.OnConnectedAsync();

        }


        private async void HandleOnStationState(StationState st, StationUpdateReason reason)
        {
            Console.WriteLine("HandleOnStationState");
            await hubContext.Clients.All.SendAsync("GetFullIPStationOKList", _client.Stations.GetFullStationList().Where(item => item.IsIPStationOK()));
        }

        public override Task OnDisconnectedAsync(Exception exception)//BAĞLANTI SONLANDIĞINDA
        {
            return base.OnDisconnectedAsync(exception);
        }

    }
}
