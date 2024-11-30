using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using RandomImageAPI.Utils;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace RandomImageAPI
{
    public class Program
    {
        public static string? RedirectURL = null;
        public static string ImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"images");
        public static bool SelfHosted = false;
        public static bool DifferentDeviceSeperated = false;
        public static string? ImageListContent = null;
        public static List<FileInfoModel> ImageList;
        public static void Main(string[] args)
        {   
            for(int i=0; i<args.Length; i++)
            {
                var a = () =>
                {
                    FileList.SaveFilesToJson(FileList.GetAllFiles(ImageFolder), AppDomain.CurrentDomain.BaseDirectory);
                };
                if (args[i].Contains("="))
                {
                    var parts = args[i].Split('=', 2);
                    string key = parts[0];
                    string value = parts[1];
                    if (key.Equals("--RedirectURL"))
                    {
                        if (value.Substring(value.Length - 1) != "/") value += "/";
                        RedirectURL = value;
                    }
                    else if (key.Equals("--ImageFolder")) {
                        ImageFolder = value;
                    }
                }
                if(args[i] == "--GenerateList" || args[i] == "-g")
                {
                    a();
                    
                }else if(args[i] == "--SelfHosted")
                {
                    SelfHosted = true;
                    a();
                }
            }
            if(RedirectURL == null && !SelfHosted) throw new NullReferenceException("You did not define --RedirectURL.");
            ImageList = JsonConvert.DeserializeObject<List<FileInfoModel>>(File.Exists("file_list.json") ? File.ReadAllText("file_list.json") : throw new FileNotFoundException());
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                // ≈‰÷√ Newtonsoft.Json µƒ—°œÓ
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Formatting = Formatting.Indented;
            });

            WebApplication app = builder.Build();
            app.UseRouting();
            app.MapControllers();
            app.Run();

        }
    }
}
