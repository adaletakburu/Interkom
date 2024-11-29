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

        public override async Task OnConnectedAsync()
        {
            _client.Connect();
            while (!_client.IsConnected)
            {
                Console.WriteLine("Waiting for connection...");
                Task.Delay(1000).Wait();
            }
            Console.WriteLine("Connected. Waiting for station data...");
            Task.Delay(10000).Wait();

            await hubContext.Clients.All.SendAsync("GetFullIPStationOKList", _client.Stations.GetFullStationList().Where(item => item.IsIPStationOK()));
            Console.WriteLine("Station Data Arrived");


            
            var test4 = _client.Stations.GetFullStationList()[0];

            _client.OnStationState += HandleOnStationState;
            _client.OnCallRequest += TestCallRequest;


            await base.OnConnectedAsync();
        }

        private async void TestCallRequest(CallRequest cr)
        {
            Console.WriteLine($"Test Call Request");
        }
        private async void HandleOnStationState(StationState st, StationUpdateReason reason)
        {
            Console.WriteLine($"Station State: {reason.ToString()} {st.RelatedStation} {st.DirectoryNumber} {st.DisplayText} {st.FullACDPMessage} {st.UniqueStationID}");
            await hubContext.Clients.All.SendAsync("GetFullIPStationOKList", _client.Stations.GetFullStationList().Where(item => item.IsIPStationOK()));
        }

        public async Task StartCall(string fromStation, string toStation)
        {
            try
            {
                _client.Connect();
                while (!_client.IsConnected)
                {
                    Console.WriteLine("Waiting for connection...");
                    Task.Delay(1000).Wait();
                }
                string command = $"$CALL {fromStation} {toStation}";
                _client.SendAlphaCommand(command);
                Console.WriteLine($"Call command sent: {command}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting call: {ex.Message}");
                throw;
            }
        }
        public async Task PlayAnnouncement(string fromStation, string announcementCode)
        {
            try
            {
                _client.Connect();
                while (!_client.IsConnected)
                {
                    Console.WriteLine("Waiting for connection...");
                    Task.Delay(1000).Wait();
                }
                string command = $"$DIAL_DAK {fromStation} {announcementCode}";
                _client.SendAlphaCommand(command);
                Console.WriteLine($"Announcement command sent: {command}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing announcement: {ex.Message}");
                throw;
            }
        }
       

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }



    }
}
