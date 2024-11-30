using System.Text.Json;

namespace RandomImageAPI.Utils
{
    public class FileList
    {
        public static List<FileInfoModel> GetAllFiles(string directoryPath)
        {
            var fileList = new List<FileInfoModel>();

            // 获取所有文件
            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                // 添加到列表
                fileList.Add(new FileInfoModel
                {
                    FileName = fileInfo.Name
                });
            }

            return fileList;
        }

        // 保存文件信息为 JSON
        public static void SaveFilesToJson(List<FileInfoModel> files, string outputPath)
        {
            // 序列化为 JSON
            var options = new JsonSerializerOptions { WriteIndented = true }; // 格式化输出
            string json = JsonSerializer.Serialize(files, options);

            // 写入文件
            File.WriteAllText(outputPath, json);
        }
    }
}
