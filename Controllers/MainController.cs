using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Reflection.Metadata.Ecma335;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;

namespace RandomImageAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class MainController : Controller
    {
        private readonly IDetectionService _detectionService;
        public MainController(IDetectionService detectionService)
        {
            _detectionService = detectionService;
        }
        public IActionResult Index()
        {
            string devicetype = "";
            var filename = Program.ImageList[Random.Shared.Next(0, Program.ImageList.Count)].FileName;
            if (Program.APISeperated) return Ok(new
            {
                Message = "You Have Enabled the Different Device API Seperated. You cannot get image from here.",
                Time = DateTime.Now
            });

            if (Program.AutoSeperate != null)
            {   

                if ((bool)Program.AutoSeperate)
                {
                    if (_detectionService.Device.Type == Device.Mobile)
                    {
                        filename = Program.MobileImagesList[Random.Shared.Next(0, Program.MobileImagesList.Count)];
                    }
                    else
                    {
                        filename = Program.PcImagesList[Random.Shared.Next(0, Program.PcImagesList.Count)];
                    }
                }
                else
                {
                    if (_detectionService.Device.Type == Device.Mobile)
                    {
                        devicetype = "mobile/";
                    }
                    else
                    {
                        devicetype = "pc/";
                    }
                }
            }
            
            if (Program.SelfHosted) {

                var path = Path.Combine(Program.ImageFolder, devicetype + filename);


                if ((bool)Program.ImageCompress)
                {
                    return File(ImageCompressed(path), "image/webp");
                }
                else
                {
                    return File(System.IO.File.ReadAllBytes(path), GetContentType(path));
                }
            } else
            {
                return Redirect($"{Program.RedirectURL}/images/{devicetype}{filename}");
            }
        }

        [HttpGet("pc")]
        public IActionResult PC()
        {
            var what = Program.AutoSeperate != null ? (!(bool)Program.AutoSeperate ? "pc/" : "") : "";
            if (Program.APISeperated)
            {var filename = Program.PcImagesList[Random.Shared.Next(0, Program.PcImagesList.Count)];
                if (Program.SelfHosted)
                {

                    var path = Path.Combine(Path.Combine(Program.ImageFolder, what + filename));

                    if (Program.ImageCompress)
                    {
                        return File(ImageCompressed(path), "image/webp");
                    }
                    else
                    {
                        return File(System.IO.File.ReadAllBytes(path), GetContentType(path));
                    }
                }
                else
                {
                    return Redirect($"{Program.RedirectURL}/images/{what}{filename}");
                }
            }
            else
            {
                return NotFound();
            }
        }
        [HttpGet("mobile")]
        public IActionResult Mobile()
        {
            var what = Program.AutoSeperate != null ? (!(bool)Program.AutoSeperate ? "mobile/" : "") : "";
            if (Program.APISeperated)
            {
                var filename = Program.MobileImagesList[Random.Shared.Next(0, Program.MobileImagesList.Count)];
                if (Program.SelfHosted)
                {

                    var path = Path.Combine(Path.Combine(Program.ImageFolder, what + filename));

                    if (Program.ImageCompress)
                    {
                        return File(ImageCompressed(path), "image/webp");
                    }
                    else
                    {
                        return File(System.IO.File.ReadAllBytes(path), GetContentType(path));
                    }
                }
                else
                {
                    return Redirect($"{Program.RedirectURL}/images/{what}{filename}");
                }
            }
            else
            {
                return NotFound();
            }
        }

        private Byte[] ImageCompressed(string path)
        {
            using (var image = Image.Load(path))
            {
                WebpEncoder encoder = new()
                {
                    Quality = Program.ImageCompressLevel,
                };

                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, encoder);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    Byte[] bytes = memoryStream.ToArray();
                    return bytes;
                }
            }
        }
        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream", // 默认 MIME 类型
            };
        }

    }
}
