using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using RandomImageAPI.Utils;
using RandomImageAPI.Impl;
using System.Runtime.CompilerServices;
using RandomImageAPI.Controllers;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Net;

namespace RandomImageAPI
{
    public class Program
    {
        public static string? RedirectURL = null;
        public static string ImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"images");
        public static bool SelfHosted = false;
        public static bool APISeperated = false;
        public static string ImageListFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file_list.json");
        public static string? ImageListContent = null;
        public static List<FileInfoModel> ImageList = new();
        public static List<string> PcImagesList = new();
        public static List<string> MobileImagesList = new();
        public static bool? AutoSeperate = null;
        public static bool ImageCompress = false;
        public static int ImageCompressLevel;
        private static bool AutoGC = false;
        private static Dictionary<string, Task> Tasks = new();
        public static void Main(string[] args)
        {
            Task task = new(() =>
            {
                int a = 1000;
                while (true)
                {
                    a = 10000;
                    var process = Process.GetCurrentProcess();
                    long workingSet = process.WorkingSet64 / 1024 / 1024;
                    if (workingSet > 1000)
                    {
                        GC.Collect();
                        a = 5000;
                    }
                    else if (workingSet > 500)
                    {
                        GC.Collect();
                        a = 10000;
                    }
                    Task.Delay(a);
                }

            });
            Tasks.Add("AutoGC", task);
            bool ListFetchType = true;
            for(int i=0; i<args.Length; i++)
            {
                var a = () =>
                {
                    FileList.SaveFilesToJson(FileList.GetAllFiles(ImageFolder), ImageListFilePath);
                };
                if (args[i].Contains("="))
                {
                    var parts = args[i].Split('=', 2);
                    string key = parts[0];
                    string value = parts[1];
                    if (key.Equals("--RedirectURL"))
                    {
                        if (value.Substring(value.Length - 1) == "/") value = value.Substring(0,value.Length - 1);
                        RedirectURL = value;
                    }
                    else if (key.Equals("--ImageFolder")) {
                        ImageFolder = value;
                    }else if (key.Equals("--Seperate"))
                    {
                        if (value == "auto")
                        {
                            AutoSeperate = true;
                        }
                        else
                        {
                            AutoSeperate = false;
                        }
                    }
                    else if (key.Equals("--ImageCompressLevel"))
                    {
                        ImageCompress = true;
                        switch (value) { 
                            case "auto":
                                ImageCompressLevel = 75;
                                break;
                            case "0":
                                ImageCompressLevel = 100;
                                break;
                            case "1":
                                ImageCompressLevel = 85;
                                break;
                            case "2":
                                ImageCompressLevel = 75;
                                break;
                            case "3":
                                ImageCompressLevel = 60;
                                break;
                            case "4":
                                ImageCompressLevel = 50;
                                break;
                            default:
                                ImageCompressLevel = 75;
                                break;
                        }
                    }
                }
                if(args[i] == "--GenerateList" || args[i] == "-g")
                {
                    a();
                    
                }else if(args[i] == "--SelfHosted")
                {
                    SelfHosted = true;
                    if (!File.Exists(ImageListFilePath)) a();

                }
                else if(args[i] == "--DeviceBasedAPISeperate")
                {
                    APISeperated = true;
                }
                else if (args[i] == "--AutoGC"){
                    AutoGC = true;
                }
            }
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddFile(o => o.RootPath = AppDomain.CurrentDomain.BaseDirectory);
            if (RedirectURL == null && !SelfHosted) throw new NullReferenceException("You did not define --RedirectURL.");
#pragma warning disable CS8601
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            ImageList = ListFetchType ? JsonConvert.DeserializeObject<List<FileInfoModel>>(File.Exists(ImageListFilePath) ? File.ReadAllText(ImageListFilePath) : throw new FileNotFoundException("Generate the file list first!!!")) : FileList.GetAllFiles(ImageFolder);
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            
            builder.Services.AddDetection();
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            builder.Services.AddControllers();
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                // ≈‰÷√ Newtonsoft.Json µƒ—°œÓ
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Formatting = Formatting.Indented;
            });
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            WebApplication app = builder.Build();
            
            app.UseRouting();
            app.MapControllers();
            app.UseMiddleware<RealIpMiddleware>();
            foreach (var item in ImageList)
            {
                if (item.Ratio > 1)
                {
                    PcImagesList.Add(item.FileName);
                    
                }
                else
                {
                    MobileImagesList.Add(item.FileName);
                }
            }
            if(AutoGC) Tasks["AutoGC"].Start();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8601
            
            

            app.Run();

        }
    }
    public class RealIpMiddleware
    {
        private readonly RequestDelegate _next;

        public RealIpMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var headers = context.Request.Headers;
            if (headers.ContainsKey("X-Forwarded-For"))
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(headers["X-Forwarded-For"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)[0]);
            }else if (headers.ContainsKey("X-Real-IP"))
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(headers["X-Real-IP"].ToString());
            }
            return _next(context);
        }
    }
}
