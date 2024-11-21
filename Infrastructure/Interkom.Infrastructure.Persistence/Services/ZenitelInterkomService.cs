using Interkom.Core.Application.Interfaces;
using NAudio.Wave;
using Stentofon.AlphaCom.AlphaNet.Client;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Interkom.Infrastructure.Persistence.Services
{
    public class ZenitelInterkomService : IZenitelIntercomService
    {
        private AlphaNetClient _client;
        public ZenitelInterkomService()
        {
            _client = new AlphaNetClient("10.0.1.15")
            {
                DoSynchronizeStates = true,
            };
            _client.Connect();
        }

        public List<string> GetFullStationList()
        {

            while (!_client.IsConnected)
            {
                Console.WriteLine("bağlı değil");
                System.Threading.Thread.Sleep(1000);
            }
            Console.WriteLine("bağlandı");

            Task.Delay(10000).Wait();
            List<string> list = new();

            foreach (var item in _client.Stations.GetFullStationList())
            {
                if (item.IsIPStationOK())
                {
                    list.Add(item.DisplayText);
                }
            }

            return list;

        }
    }
}
