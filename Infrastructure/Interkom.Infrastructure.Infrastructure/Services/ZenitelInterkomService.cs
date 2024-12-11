using Interkom.Core.Application.Interfaces;
using Stentofon.AlphaCom.AlphaNet.Client;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace Interkom.Infrastructure.Infrastructure.Services
{
    public class ZenitelInterkomService : IZenitelIntercomService
    {
        private AlphaNetClient _client = new AlphaNetClient("10.0.1.20")
        {
            DoSynchronizeStates = true,
        };
        private readonly HttpClient _httpClient;
        private readonly ILogger<ZenitelInterkomService> _logger;
        public ZenitelInterkomService(AlphaNetClient client, HttpClient httpClient, ILogger<ZenitelInterkomService> logger)
        {
            _client = client;
            _httpClient = httpClient;
            _logger = logger;
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

        public async Task<bool> UploadAudioFileAsync(string ip, string username, string password, string filePath, string group, int index)
        {
            try
            {
                // 1. CSRF token ve PHPSESSID'yi almak için GET isteği gönder.
                var csrfRequest = new HttpRequestMessage(HttpMethod.Get, $"http://{ip}/php/amc_wav_upload.php");
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                csrfRequest.Headers.Add("Authorization", $"Basic {credentials}");

                var csrfResponse = await _httpClient.SendAsync(csrfRequest);
                csrfResponse.EnsureSuccessStatusCode();

                var csrfContent = await csrfResponse.Content.ReadAsStringAsync();
                var csrfToken = Regex.Match(csrfContent, "<input[^>]*name=\"csrf_token\"[^>]*value=\"(?<token>[^\"]+)\"", RegexOptions.IgnoreCase).Groups["token"].Value;

                var sessionCookie = csrfResponse.Headers.GetValues("Set-Cookie")
                    .FirstOrDefault(c => c.StartsWith("PHPSESSID="))?
                    .Split(';')?[0]?.Replace("PHPSESSID=", "");

                if (string.IsNullOrEmpty(csrfToken) || string.IsNullOrEmpty(sessionCookie))
                {
                    Console.WriteLine("CSRF token veya PHPSESSID alınamadı.");
                    return false;
                }

                // 2. Ses dosyasını POST isteği ile yükle.
                using var formData = new MultipartFormDataContent();

                // userfile parametresi için dosya içeriği ekleniyor.
                using var fileStream = File.OpenRead(filePath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                formData.Add(fileContent, "userfile", Path.GetFileName(filePath));

                // Diğer parametreler
                formData.Add(new StringContent(csrfToken), "csrf_token");
                formData.Add(new StringContent("1"), "msg_type_action");
                formData.Add(new StringContent(index.ToString()), "msg_ind");
                formData.Add(new StringContent(group), "msg_type");

                var postRequest = new HttpRequestMessage(HttpMethod.Post, $"http://{ip}/php/amc_wav_upload.php")
                {
                    Content = formData
                };

                postRequest.Headers.Add("Authorization", $"Basic {credentials}");
                postRequest.Headers.Add("Cookie", $"PHPSESSID={sessionCookie}");

                var postResponse = await _httpClient.SendAsync(postRequest);
                postResponse.EnsureSuccessStatusCode();

                Console.WriteLine("Ses dosyası başarıyla yüklendi.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return false;
            }
        }


    }
}
