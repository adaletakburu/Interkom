using Interkom.Core.Application.Interfaces;
using Stentofon.AlphaCom.AlphaNet.Client;
using System;
using System.Diagnostics;

namespace Interkom.Infrastructure.Infrastructure.Services
{
    public class ZenitelInterkomService : IZenitelIntercomService
    {
        private AlphaNetClient _client = new AlphaNetClient("10.0.1.20")
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

        public void MakeAnnounce(string command)
        {
            _client.SendAlphaCommand(command);
        }

        public bool SendAudioToIntercom(string filePath, string rtpAddress)
        {
            try
            {
                // ffmpeg komutu oluşturuluyor
                string ffmpegPath = @"C:\ffmpeg\bin\ffmpeg.exe"; // ffmpeg'in yolu
                string arguments = $"-re -i \"{filePath}\" -ac 1 -acodec g722 -ar 16000 -filter:a volume=-6dB -f rtp {rtpAddress}";

                // Process başlatılıyor
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            Console.WriteLine($"Output: {e.Data}");
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (string.IsNullOrEmpty(e.Data))
                        {
                            Console.WriteLine($"Info: {e.Data}");
                        }
                    };

                    process.Start();
                    _client.SendAlphaCommand("$CONF L86 L8202");

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                    _client.SendAlphaCommand("$CONF L86 L8200");

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("ffmpeg komutu başarıyla çalıştı:");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("ffmpeg komutu hata ile sonlandı:");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bir hata oluştu: {ex.Message}");
                return false;
            }
        }

    }
}
