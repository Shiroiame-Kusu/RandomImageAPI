using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SkiaSharp;
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
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Byte[] bytes = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            var a = () =>{
                using var inputStream = System.IO.File.OpenRead(path);
                using var bitmap = SKBitmap.Decode(inputStream);
                using var image = SKImage.FromBitmap(bitmap);
                using var webpData = image.Encode(SKEncodedImageFormat.Webp, Program.ImageCompressLevel);
                if (System.IO.File.Exists(path + ".ccache")) System.IO.File.Delete(path + ".ccache");
                bytes = webpData.ToArray();
                System.IO.File.WriteAllBytes(path + ".ccache",bytes);
            };

            if (!System.IO.File.Exists(path + ".ccache")) {
                a();
            }
            else
            {
                try
                {
                    using Image m = Image.Load(path + ".ccache");
                    bytes = System.IO.File.ReadAllBytes(path + ".ccache");
                }
                catch
                {
                    a();
                }
            }
#pragma warning disable CS8603 // Possible null reference return.
            return bytes;
#pragma warning restore CS8603 // Possible null reference return.

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
