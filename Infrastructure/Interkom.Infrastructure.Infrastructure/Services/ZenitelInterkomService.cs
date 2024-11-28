using Interkom.Core.Application.Interfaces;
using Stentofon.AlphaCom.AlphaNet.Client;

namespace Interkom.Infrastructure.Infrastructure.Services
{
    public class ZenitelInterkomService : IZenitelIntercomService
    {
        private AlphaNetClient _client = new AlphaNetClient("10.0.1.15")
        {
            DoSynchronizeStates = true,
        };

        public ZenitelInterkomService(AlphaNetClient client)
        {
            _client = client;
        }

        public List<string> GetFullStationList()
        {

            _client.Connect();
            while (!_client.IsConnected)
            {
                Console.WriteLine("bağlı değil");
                System.Threading.Thread.Sleep(1000);
            }
            Console.WriteLine("bağlandı");

            Task.Delay(10000).Wait();
           List<string> list = new();
            _client.SendAlphaCommand("$CALL L102 L105");
            //var x = _client.GetNodeStationStates(1);
            return list;

        }
    }
}
