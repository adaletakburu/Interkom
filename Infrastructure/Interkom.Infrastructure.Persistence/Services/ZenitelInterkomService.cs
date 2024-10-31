using Interkom.Core.Application.Interfaces;
using NAudio.Wave;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Interkom.Infrastructure.Persistence.Services
{
    public class ZenitelInterkomService : IZenitelIntercomService
    {

        private UdpClient rtpUdpClient;
        private WaveInEvent waveIn;
        private ushort sequenceNumber;
        private uint timestamp;

        public async Task StartAudioTransmission()
        {
            rtpUdpClient = new UdpClient();
            rtpUdpClient.Connect("10.1.58.85", 16384);


            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(8000, 16, 1)
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveIn.StartRecording();

        }

        public void StopAudioTransmission()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }

            if (rtpUdpClient != null)
            {
                rtpUdpClient.Dispose();
                rtpUdpClient = null;
            }

            Console.WriteLine("Audio transmission stopped.");
        }


        private async void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // 16-bit PCM formatındaki ham ses verisini 8-bit G.711 PCMA'ya dönüştür
            byte[] g711Data = Convert16BitPcmToG711(e.Buffer);

            // Dönüştürülmüş veriyi RTP paketiyle gönder
            await SendRtpPacket(g711Data, timestamp, sequenceNumber);

            // Timestamp ve sequence number'ı güncelle
            timestamp += (uint)g711Data.Length; // Bu örnekte her byte başına 1 artış
            sequenceNumber++;
        }


        private byte[] Convert16BitPcmToG711(byte[] pcmData)
        {
            int length = pcmData.Length / 2;
            byte[] g711Data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                short sample = (short)((pcmData[i * 2 + 1] << 8) | pcmData[i * 2]);
                g711Data[i] = LinearToALawSample(sample);
            }
            return g711Data;
        }

        private byte LinearToALawSample(short sample)
        {
            int sign = (sample >> 8) & 0x80;
            if (sign != 0) sample = (short)-sample;
            if (sample > 32635) sample = 32635;

            int exponent = 7;
            for (int expMask = 0x4000; (sample & expMask) == 0 && exponent > 0; exponent--, expMask >>= 1) ;
            int mantissa = (sample >> ((exponent == 0) ? 4 : (exponent + 3))) & 0x0F;
            byte alawByte = (byte)(sign | (exponent << 4) | mantissa);
            return (byte)~alawByte;
        }

        private async Task SendRtpPacket(byte[] audioData, uint timestamp, ushort sequenceNumber)
        {
            byte[] rtpHeader = new byte[12];
            rtpHeader[0] = 0x80;
            rtpHeader[1] = 0x08;
            rtpHeader[2] = (byte)(sequenceNumber >> 8);
            rtpHeader[3] = (byte)(sequenceNumber & 0xFF);
            rtpHeader[4] = (byte)(timestamp >> 24);
            rtpHeader[5] = (byte)((timestamp >> 16) & 0xFF);
            rtpHeader[6] = (byte)((timestamp >> 8) & 0xFF);
            rtpHeader[7] = (byte)(timestamp & 0xFF);

            uint ssrc = 0x3f2d5269;
            rtpHeader[8] = (byte)(ssrc >> 24);
            rtpHeader[9] = (byte)((ssrc >> 16) & 0xFF);
            rtpHeader[10] = (byte)((ssrc >> 8) & 0xFF);
            rtpHeader[11] = (byte)(ssrc & 0xFF);

            byte[] rtpPacket = new byte[rtpHeader.Length + audioData.Length];
            Buffer.BlockCopy(rtpHeader, 0, rtpPacket, 0, rtpHeader.Length);
            Buffer.BlockCopy(audioData, 0, rtpPacket, rtpHeader.Length, audioData.Length);

            await rtpUdpClient.SendAsync(rtpPacket, rtpPacket.Length);

            Console.WriteLine($"RTP packet sent. Sequence Number: {sequenceNumber}, Timestamp: {timestamp}, SSRC: {ssrc:X}");
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
            string sipMessage = @"INVITE sip:23@10.1.58.85 SIP/2.0
Via: SIP/2.0/UDP 10.1.58.86:65119;rport;branch=z9hG4bKPj5d74dd9a7e87479bafbc23a2c547dd55
Max-Forwards: 70
From: <sip:admin@10.1.58.85>;tag=6930f97b2a5e4cd9b45c643312f1b696
To: <sip:23@10.1.58.85>
Contact: <sip:admin@10.1.58.86:65119;ob>
Call-ID: 00a57ac84c7f4c29aece388f5bee29b3
CSeq: 2843 INVITE
Allow: PRACK, INVITE, ACK, BYE, CANCEL, UPDATE, INFO, SUBSCRIBE, NOTIFY, REFER, MESSAGE, OPTIONS
Supported: replaces, 100rel, timer, norefersub
Session-Expires: 1800
Min-SE: 90
User-Agent: MicroSIP/3.21.5
Content-Type: application/sdp
Content-Length:   336

v=0
o=- 3939101195 3939101195 IN IP4 10.2.149.10
s=pjmedia
b=AS:84
t=0 0
a=X-nat:0
m=audio 4064 RTP/AVP 8 0 101
c=IN IP4 10.2.149.10
b=TIAS:64000
a=rtcp:4065 IN IP4 10.2.149.10
a=sendrecv
a=rtpmap:8 PCMA/8000
a=rtpmap:0 PCMU/8000
a=rtpmap:101 telephone-event/8000
a=fmtp:101 0-16
a=ssrc:1903311638 cname:62d42ead13086271
";


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

            string ackMessage = "ACK sip:23@10.1.58.85:5060 SIP/2.0\r\n" +
                      "Via: SIP/2.0/UDP 10.1.58.86:65119;branch=z9hG4bKPjf6c3ea54c9c54f0c806a151483cd4e90\r\n" +
                      "From: <sip:admin@10.1.58.85>;tag=6930f97b2a5e4cd9b45c643312f1b696\r\n" +
                      "To: <sip:23@10.1.58.85;tag=" + isTag + ">\r\n" +
                      "Call-ID: 00a57ac84c7f4c29aece388f5bee29b3\r\n" +
                      "CSeq: 2843 ACK\r\n" +
                      "Contact: <sip:admin@10.1.58.86:65119;ob>\r\n\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(ackMessage);

            using (var udpClient = new UdpClient())
            {
                await udpClient.SendAsync(messageBytes, messageBytes.Length, serverEndpoint);
                Console.WriteLine("SIP ACK mesajı gönderildi.");
            }

            await StartAudioTransmission();
        }

        public async Task CancelAsync()
        {
            var serverEndpoint = new IPEndPoint(IPAddress.Parse("10.1.58.85"), 5060);

            string cancelMessage = "CANCEL sip:bob@10.1.58.85 SIP/2.0\r\n" +
                                   "Via: SIP/2.0/UDP 10.1.58.88:5060\r\n" +
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
                Console.WriteLine("SIP CANCEL mesajı gönderildi.");
            }
        }

        public async Task OptionsAsync()
        {
            var serverEndpoint = new IPEndPoint(IPAddress.Parse("10.1.58.85"), 5060);

            string optionsMessage = "OPTIONS sip:bob@10.1.58.85 SIP/2.0\r\n" +
                                    "Via: SIP/2.0/UDP 10.1.58.88:5060\r\n" +
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

            string byeMessage = "BYE sip:23@10.1.58.85 SIP/2.0\r\n" +
               "Via: SIP/2.0/UDP 10.1.58.86:65119;branch=z9hG4bKPjf6c3ea54c9c54f0c806a151483cd4e90\r\n" +
               "From: <sip:admin@10.1.58.85>;tag=6930f97b2a5e4cd9b45c643312f1b696\r\n" +
               "To: <sip:23@10.1.58.85>;tag=" + isTag + ">\r\n" + // toTag değeri burada dinamik olarak eklenebilir
               "Call-ID: 00a57ac84c7f4c29aece388f5bee29b3\r\n" +
               "CSeq: 2844 BYE\r\n" +
               "Contact: <sip:admin@10.1.58.86:65119;ob>\r\n" +
               "Content-Length: 0\r\n\r\n";


            byte[] messageBytes = Encoding.UTF8.GetBytes(byeMessage);

            using (var udpClient = new UdpClient())
            {
                await udpClient.SendAsync(messageBytes, messageBytes.Length, serverEndpoint);
                Console.WriteLine("SIP BYE mesajı gönderildi.");
            }
            StopAudioTransmission();
        }
    }
}
