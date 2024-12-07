using Microsoft.AspNetCore.Mvc;
using RandomImageAPI.Impl;
using RandomImageAPI.Utils;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;
using File2 = System.IO.File;

namespace RandomImageAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class MainController : Controller
    {
        private readonly IDetectionService _detectionService;
        private static List<MemoryStream> memoryStreams = new();
        private static ConcurrentDictionary<string, Byte[]> PCCaches = new();
        private static Dictionary<string, Byte[]> MobileCaches = new();
        private static Stopwatch stopwatch = new();
        public MainController(IDetectionService detectionService)
        {
            _detectionService = detectionService;
            ClassAccess.MainController = this;

        }
        public async Task<IActionResult> Index()
        {
            LogInfo(Request);
            string devicetype = "";
            var filename = Program.ImageList[Random.Shared.Next(0, Program.ImageList.Count)].FileName;
            if (Program.APISeperated) return Ok(new
            {
                Message = "You Have Enabled the Device Based API Seperating. You cannot get image from here.",
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


                if (Program.ImageCompress)
                {
                    return File(ImageCompressed(path, _detectionService.Device.Type), "image/webp");
                }
                else
                {
                    return File(File2.ReadAllBytes(path), GetContentType(path));
                }
            } else
            {
                return Redirect($"{Program.RedirectURL}/images/{devicetype}{filename}");
            }
        }
        static Byte[]? bytes2 = null;
        [HttpGet("pc")]
        public async Task<IActionResult> PC()
        {
            LogInfo(Request);
            bytes2 = null;
            var what = Program.AutoSeperate != null ? (!(bool)Program.AutoSeperate ? "pc/" : "") : "";
            if (Program.APISeperated)
            {
                var filename = Program.PcImagesList[Random.Shared.Next(0, Program.PcImagesList.Count)];
                if (Program.SelfHosted)
                {

                    var path = Path.Combine(Path.Combine(Program.ImageFolder, what + filename));

                    if (Program.ImageCompress)
                    {
                        
                        if (PCCaches.TryGetValue(filename, out bytes2))
                        {
                            return File(bytes2, "image/webp");
                        }
                        else
                        {
                            return File(ImageCompressed(path,Device.Desktop), "image/webp");
                            
                        }

                    }
                    else
                    {
                        return File(File2.ReadAllBytes(path), GetContentType(path));
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

        static Byte[]? bytes3 = null;
        [HttpGet("mobile")]
        public async Task<IActionResult> Mobile()
        {
            LogInfo(Request);
            bytes3 = null;
            var what = Program.AutoSeperate != null ? (!(bool)Program.AutoSeperate ? "mobile/" : "") : "";
            if (Program.APISeperated)
            {
                var filename = Program.MobileImagesList[Random.Shared.Next(0, Program.MobileImagesList.Count)];
                if (Program.SelfHosted)
                {

                    var path = Path.Combine(Path.Combine(Program.ImageFolder, what + filename));

                    if (Program.ImageCompress)
                    {
                        if (MobileCaches.TryGetValue(filename, out bytes3))
                        {
                            return File(bytes3, "image/webp");
                        }
                        else
                        {
                            return File(ImageCompressed(path, Device.Mobile), "image/webp");
                        }
                    }
                    else
                    {
                        return File(File2.ReadAllBytes(path), GetContentType(path));
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
        static Byte[]? bytes;
        private static Byte[] ImageCompressed(string path,Device type)
        {
            bytes = null;
            if (!File2.Exists(path + ".ccache")) {
                ImageCompressedA(path, type);
            }
            else
            {
                try
                {
                    using (var image = SKBitmap.Decode(path + ".ccache"))
                    {
                        if (image == null)
                        {
                            File2.Delete(path + ".ccache");
                            throw new Exception("Cache is broken, Will generate new one next time.");
                        }
                    }
                    bytes = File2.ReadAllBytes(path + ".ccache");
                    if(type == Device.Mobile)
                    {
                        MobileCaches.Add(Path.GetFileName(path),bytes);
                    }
                    else
                    {
                        PCCaches.TryAdd(Path.GetFileName(path), bytes);
                    }
                }
                catch(Exception ex)
                {
                    Console.Write(ex);
                    ImageCompressedA(path,type);
                }
            }
#pragma warning disable CS8603 // Possible null reference return.
            return bytes;
#pragma warning restore CS8603 // Possible null reference return.

        }
        private static void ImageCompressedA(string path, Device type)
        {
            using var webpData = SKImage.FromBitmap(SKBitmap.Decode(File2.OpenRead(path))).Encode(SKEncodedImageFormat.Webp, Program.ImageCompressLevel);
            if (File2.Exists(path + ".ccache")) File2.Delete(path + ".ccache");
            bytes = webpData.ToArray();
            File2.WriteAllBytes(path + ".ccache", bytes);
            if(type != Device.Mobile)
            {   
                if(!PCCaches.ContainsKey(Path.GetFileName(path)))
                PCCaches.TryAdd(Path.GetFileName(path), bytes);
            }
            else
            {
                if (!MobileCaches.ContainsKey(Path.GetFileName(path)))
                    MobileCaches.Add(Path.GetFileName(path), bytes);
            }
            GC.Collect();
        }
        private static string GetContentType(string path)
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
        private static void LogInfo(HttpRequest request)
        {
            Console.WriteLine($"{request.Host} {request.Path} {request.Protocol} {request.Headers.UserAgent}");
        }
    }
}
