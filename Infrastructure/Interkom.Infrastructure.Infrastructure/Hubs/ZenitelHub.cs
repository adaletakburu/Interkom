﻿using Microsoft.AspNetCore.SignalR;
using Stentofon.AlphaCom.AlphaNet.Client;
using Stentofon.AlphaCom.AlphaNet.Messages;

namespace Interkom.Infrastructure.Infrastructure.Hubs
{
    public class ZenitelHub : Hub
    {
        private readonly AlphaNetClient _client;
        private readonly IHubContext<ZenitelHub> hubContext;

        public ZenitelHub(IHubContext<ZenitelHub> context, AlphaNetClient client)
        {
            hubContext = context;
            _client = client;
        }


        public override async Task OnConnectedAsync()//BAĞLANTI SAĞLANDIĞINDA
        {

            await hubContext.Clients.All.SendAsync("GetFullIPStationOKList", _client.Stations.GetFullStationList().Where(item => item.IsIPStationOK()));
            _client.OnStationState += HandleOnStationState;
            _client.OnCallStatus += HandleOnCallStatus;//ARAMA İŞLEMİ OLDUĞUNDAKİ KİM KİMİ ARIYOR BİLGİSİ

            //_client.OnOpenCallStatus += HandleOnOpenCallStatus;
            //_client.OnPrivateCallStatus += HandleOnPrivateCallStatus;
            //_client.OnBusyCallStatus += HandleOnBusyCallStatus;// ARAMA YAPTIĞI KİŞİ BAŞKASIYA GÖRÜŞÜYORSA (MEŞGULSE)
            //_client.OnCallRequestsSynchronized += HandleOnCallRequestsSynchronized;

            await base.OnConnectedAsync();

        }


        private void HandleOnCallRequestsSynchronized()
        {
            var callrequests = _client.Stations.GetStationWithCallRequestsCount(0);
            Console.WriteLine("Stations Ready, Call Requests Ready ({0})", callrequests);
        }

        private void HandleOnBusyCallStatus(CallStatus cs)
        {
            Console.Write(String.Format("Stations: HandleOnBusyCallStatus event. Time: {0}\r\n", cs.CallTime.ToString()));
        }

        private void HandleOnPrivateCallStatus(CallStatus cs)
        {
            Console.Write(String.Format("Stations: HandleOnPrivateCallStatus event. Time: {0}\r\n", cs.CallTime.ToString()));
        }

        private void HandleOnOpenCallStatus(CallStatus cs)
        {
            Console.Write(String.Format("Stations: HandleOnOpenCallStatus event. Time: {0}\r\n", cs.CallTime.ToString()));
        }

        private async void HandleOnCallStatus(CallStatus cs)
        {
            Console.Write(String.Format("Stations: HandleOnCallStatus event. Time: {0}\r\n", cs.CallTime.ToString()));
            Console.WriteLine($"Arayan: {cs.StationA.DigitString} , Aranan: {cs.StationB.DigitString}");
            await hubContext.Clients.All.SendAsync("HandleOnCallStatus", cs);
        }

        private async void HandleOnStationState(StationState st, StationUpdateReason reason)
        {
            if (StationUpdateReason.StateNotify == reason)
                await hubContext.Clients.All.SendAsync("GetFullIPStationOKList", _client.Stations.GetFullStationList().Where(item => item.IsIPStationOK()));

            if (st.RelatedStation?.DigitString == null && StationUpdateReason.ConnectBC == reason)
                await hubContext.Clients.All.SendAsync("StopBlink", st.DirectoryNumber.DigitString);
        }


        public async Task Call(string digitNumber)
        {
            string command = "$CALL L101 L" + digitNumber;
            _client.SendAlphaCommand(command);
            Task.Delay(10000).Wait();
            _client.SendAlphaCommand("$CAC");
        }



        public override Task OnDisconnectedAsync(Exception exception)//BAĞLANTI SONLANDIĞINDA
        {
            return base.OnDisconnectedAsync(exception);
        }

    }
}