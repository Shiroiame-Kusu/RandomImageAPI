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
        public static bool APISeperated = false;
        public static string ImageListFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file_list.json");
        public static string? ImageListContent = null;
        public static List<FileInfoModel> ImageList = new();
        public static List<string> PcImagesList = new();
        public static List<string> MobileImagesList = new();
        public static bool? AutoSeperate = null;
        public static void Main(string[] args)
        {
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
            }
            if(RedirectURL == null && !SelfHosted) throw new NullReferenceException("You did not define --RedirectURL.");
            ImageList = ListFetchType ? JsonConvert.DeserializeObject<List<FileInfoModel>>(File.Exists(ImageListFilePath) ? File.ReadAllText(ImageListFilePath) : throw new FileNotFoundException("Generate the file list first!!!")) : FileList.GetAllFiles(ImageFolder);
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
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDetection();
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
