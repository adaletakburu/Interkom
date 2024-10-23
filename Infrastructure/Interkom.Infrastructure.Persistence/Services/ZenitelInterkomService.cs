using Interkom.Core.Application.Interfaces;
using SIPSorcery.Media;
using SIPSorcery.SIP.App;
using SIPSorceryMedia.Windows;

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
    }
}
