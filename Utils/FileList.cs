using Newtonsoft.Json;
using SixLabors.ImageSharp;


namespace RandomImageAPI.Utils
{
    public class FileList
    {
        public static List<FileInfoModel> GetAllFiles(string directoryPath)
        {
            var fileList = new List<FileInfoModel>();

            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    using (Image image = Image.Load(file))
                    {
                        int width = image.Width;
                        int height = image.Height;

                        double ratio = Math.Round((double)width / height, 2);

                        fileList.Add(new FileInfoModel
                        {
                            FileName = fileInfo.Name,
                            Ratio = ratio,

                        });

                    }

                }
            

            return fileList;
        }

        public static void SaveFilesToJson(List<FileInfoModel> files, string outputPath)
        {
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(files, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }
    }
}
