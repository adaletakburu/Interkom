using Interkom.Core.Application.Interfaces;
using Interkom.Presentation.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Interkom.Presentation.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IZenitelIntercomService _service;
        public HomeController(ILogger<HomeController> logger, IZenitelIntercomService service)
        {
            _logger = logger;
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [HttpPost]
        public IActionResult MakeAnnounce(string command)
        {
            _service.MakeAnnounce(command);
            return Ok();
        }

        [HttpPost]
        public bool FfmpegTest() 
        {
            string filePath = @"C:\ffmpeg\media\anons2.mp3";
            string rtpAddress = "rtp://10.0.1.20:4004";

            bool result = _service.SendAudioToIntercom(filePath, rtpAddress);

            if (result)
            {
                Console.WriteLine("Ses interkoma başarıyla gönderildi.");
                return true;
            }
            else
            {
                Console.WriteLine("Ses gönderimi sırasında bir hata oluştu.");
                return false;
            }
        }

        [HttpPost]
        public IActionResult SendAudioToIntercom(IFormFile audioFile, string rtpAddress)
        {
            if (audioFile == null || string.IsNullOrEmpty(rtpAddress))
            {
                return BadRequest("Ses dosyası ve RTP adresi gerekli.");
            }

            try
            {
                // Geçici bir dosyaya kaydedin
                string tempFilePath = Path.Combine(Path.GetTempPath(), audioFile.FileName);

                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    audioFile.CopyTo(stream);
                }

                // Servis ile ses dosyasını gönderin
                bool success = _service.SendAudioToIntercom(tempFilePath, rtpAddress);

                // Dosyayı işlendikten sonra temizleyin
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }

                if (success)
                {
                    return Ok("Ses dosyası başarıyla gönderildi.");
                }
                else
                {
                    return StatusCode(500, "Ses dosyası gönderilemedi.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
