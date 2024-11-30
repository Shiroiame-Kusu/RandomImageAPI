using Microsoft.AspNetCore.Mvc;

namespace RandomImageAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            if (Program.SelfHosted) {
                
                var path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images"), Program.ImageList[Random.Shared.Next(0, Program.ImageList.Count - 1)].FileName);
                return File(System.IO.File.ReadAllBytes(path), GetContentType(path));
            }

            if (Program.DifferentDeviceSeperated)
            {   
                return Ok(new
                {
                    Message = "How Dare You",
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                return Redirect(Program.RedirectURL +"");
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
