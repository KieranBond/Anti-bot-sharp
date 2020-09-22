using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AntiBotSharp.Helpers
{
    public class FileHelper
    {
        private static string EnvironmentPath
        {
            get 
            {
                if(_environmentPath == null)
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                return _environmentPath;
            }
        }

        private static string _environmentPath;

        public static async Task<T> GetFromJSONFileAsync<T>(string filename)
        {
            return await GetFromJSONFileAsync<T>(filename, EnvironmentPath);
        }

        public static async Task<T> GetFromJSONFileAsync<T>(string filename, string environmentPath)
        {
            if (!Path.HasExtension(filename))
                filename = Path.ChangeExtension(filename, ".json");

            string filepath = Path.Combine(environmentPath, filename);
            
            if (!File.Exists(filepath))
                return default(T);

            string fileJSON = await File.ReadAllTextAsync(filepath);

            return JsonConvert.DeserializeObject<T>(fileJSON);
        }
    }
}
