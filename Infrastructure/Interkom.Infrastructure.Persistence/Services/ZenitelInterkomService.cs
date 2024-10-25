using Interkom.Core.Application.Interfaces;
using SIPSorcery.Media;
using SIPSorcery.SIP.App;
using SIPSorceryMedia.Windows;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics.Metrics;

namespace Interkom.Infrastructure.Persistence.Services
{
    public class ZenitelInterkomService : IZenitelIntercomService
    {
        static string DESTINATION = "10.1.58.85";
        static SIPUserAgent userAgent = new SIPUserAgent();

        public async Task CallAsync()
        {
            var winAudioEndPoint = new WindowsAudioEndPoint(new AudioEncoder());
            var voipMediaSession = new VoIPMediaSession(winAudioEndPoint.ToMediaEndPoints());

            bool callResult = await userAgent.Call(DESTINATION, null, null, voipMediaSession);

            Console.WriteLine("Press any key to exit...");
            //Console.ReadLine();
            throw new NotImplementedException();
        }

        public Task ClientsAsync()
        {
            userAgent.Hangup();//HANGUP
            throw new NotImplementedException();
        }

        public Task MessagesAsync()
        {
            throw new NotImplementedException();
        }


        public Task StateAsync()
        {
            throw new NotImplementedException();
        }

        public Task StationStateAsync()
        {
            throw new NotImplementedException();
        }

        static string isTag = null;
        static string TagAl(string metin)
        {
            // Regex deseni, 'tag=' ile başlayan ve ardından gelen değeri alır.
            string desen = @"tag=([^;]*)";
            Match eslesme = Regex.Match(metin, desen);

            if (eslesme.Success)
            {
                isTag = eslesme.Groups[1].Value;
                return eslesme.Groups[1].Value; // Eşleşen değeri döner.
            }

            return null; // Eşleşme yoksa null döner.
        }

        private async void ListenForMessagesAsync()
        {
            using (var udpServer = new UdpClient(5060))
            {
                Console.WriteLine("SIP sunucu 5060 portunda bekliyor...");

                while (true)
                {
                    var receivedResult = await udpServer.ReceiveAsync();
                    string receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);
                    if (receivedMessage.Contains("SIP/2.0 200 OK"))
                    {

                        Console.WriteLine("SIP mesajı alındı:");
                        Console.WriteLine(receivedMessage);
                        TagAl(receivedMessage);
                        break;
                    }
                    else if (receivedMessage.Contains("SIP/2.0 486 Busy Here"))
                    {

                        Console.WriteLine("SIP mesajı alındı:");
                        Console.WriteLine(receivedMessage);
                        break;
                    }
                }
            }
        }

        public async Task InviteAsync()
        {
            var serverEndpoint = new IPEndPoint(IPAddress.Parse("10.1.58.85"), 5060);

            // Gönderilecek basit bir SIP INVITE mesajı
            string sipMessage = "INVITE sip:bob@10.1.58.85 SIP/2.0\r\n" +
                                "Via: SIP/2.0/UDP 192.168.1.10:5060\r\n" +
                                "Max-Forwards: 70\r\n" +
                                "To: <sip:bob@10.1.58.85>\r\n" +
                                "From: <sip:alice@10.1.58.85>\r\n" +
                                "Call-ID: 12345@10.1.58.85\r\n" +
                                "CSeq: 1 INVITE\r\n" +
                                "Contact: <sip:alice@10.1.58.85>\r\n" +
                                "Content-Length: 0\r\n\r\n";

            byte[] messageBytes = Encoding.UTF8.GetBytes(sipMessage);

            using (var udpClient = new UdpClient())
            {
                udpClient.Send(messageBytes, messageBytes.Length, serverEndpoint);
                Console.WriteLine("SIP INVITE mesajı gönderildi.");
            }

            ListenForMessagesAsync();
        }

        public async Task AckAsync()
        {
            var serverEndpoint = new IPEndPoint(IPAddress.Parse("10.1.58.85"), 5060);

            string ackMessage = "ACK sip:bob@10.1.58.85 SIP/2.0\r\n" +
                                "Via: SIP/2.0/UDP 192.168.1.10:5060\r\n" +
                                "Max-Forwards: 70\r\n" +
                                $"To: <sip:bob@10.1.58.85>;tag={isTag}\r\n" +
                                "From: <sip:alice@10.1.58.85>\r\n" +
                                "Call-ID: 12345@10.1.58.85\r\n" +
                                "CSeq: 1 ACK\r\n" +
                                "Contact: <sip:alice@10.1.58.85>\r\n" +
                                "Content-Length: 0\r\n\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(ackMessage);

            using (var udpClient = new UdpClient())
            {
                await udpClient.SendAsync(messageBytes, messageBytes.Length, serverEndpoint);
                Console.WriteLine("SIP ACK mesajı gönderildi.");
            }

        }

        public async Task CancelAsync()
        {
            var serverEndpoint = new IPEndPoint(IPAddress.Parse("10.1.58.85"), 5060);

            string cancelMessage = "CANCEL sip:bob@10.1.58.85 SIP/2.0\r\n" +
                                   "Via: SIP/2.0/UDP 192.168.1.10:5060\r\n" +
                                   "Max-Forwards: 70\r\n" +
                                    $"To: <sip:bob@10.1.58.85>;tag={isTag}\r\n" +
                                   "From: <sip:alice@10.1.58.85>\r\n" +
                                   "Call-ID: 12345@10.1.58.85\r\n" +
                                   "CSeq: 1 CANCEL\r\n" +
                                   "Contact: <sip:alice@10.1.58.85>\r\n" +
                                   "Content-Length: 0\r\n\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(cancelMessage);

            using (var udpClient = new UdpClient())
            {
                await udpClient.SendAsync(messageBytes, messageBytes.Length, serverEndpoint);
                Console.WriteLine("SIP ACCANCELK mesajı gönderildi.");
            }
        }

        public async Task OptionsAsync()
        {
            var serverEndpoint = new IPEndPoint(IPAddress.Parse("10.1.58.85"), 5060);

            string optionsMessage = "OPTIONS sip:bob@10.1.58.85 SIP/2.0\r\n" +
                                    "Via: SIP/2.0/UDP 192.168.1.10:5060\r\n" +
                                    "Max-Forwards: 70\r\n" +
                                    $"To: <sip:bob@10.1.58.85>;tag={isTag}\r\n" +
                                    "From: <sip:alice@10.1.58.85>\r\n" +
                                    "Call-ID: 12345@10.1.58.85\r\n" +
                                    "CSeq: 1 OPTIONS\r\n" +
                                    "Contact: <sip:alice@10.1.58.85>\r\n" +
                                    "Accept: application/sdp\r\n" +
                                    "Content-Length: 0\r\n\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(optionsMessage);

            using (var udpClient = new UdpClient())
            {
                await udpClient.SendAsync(messageBytes, messageBytes.Length, serverEndpoint);
                Console.WriteLine("SIP OPTIONS mesajı gönderildi.");
            }
        }

        public async Task ByeAsync()
        {
            var serverEndpoint = new IPEndPoint(IPAddress.Parse("10.1.58.85"), 5060);

            string byeMessage = "BYE sip:bob@10.1.58.85 SIP/2.0\r\n" +
                                "Via: SIP/2.0/UDP 192.168.1.10:5060\r\n" +
                                "Max-Forwards: 70\r\n" +
                                $"To: <sip:bob@10.1.58.85>;tag={isTag}\r\n" +
                                "From: <sip:alice@10.1.58.85>\r\n" +
                                "Call-ID: 12345@10.1.58.85\r\n" +
                                "CSeq: 2 BYE\r\n" +
                                "Contact: <sip:alice@10.1.58.85>\r\n" +
                                "Content-Length: 0\r\n\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(byeMessage);

            using (var udpClient = new UdpClient())
            {
                await udpClient.SendAsync(messageBytes, messageBytes.Length, serverEndpoint);
                Console.WriteLine("SIP BYE mesajı gönderildi.");
            }
        }
    }
}
