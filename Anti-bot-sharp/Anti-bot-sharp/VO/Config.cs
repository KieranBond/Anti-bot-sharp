using AntiBotSharp.Helpers;
using System;
using System.Threading.Tasks;

namespace AntiBotSharp.VO
{
    [Serializable]
    public class Config
    {
        public string Token { get; set; }
        public string[] Admins { get; set; }

        public static async Task<Config> BuildConfig()
        {
            var config = await LoadConfigFromFile();

            return config;
        }

        private static async Task<Config> LoadConfigFromFile()
        {
            return await FileHelper.GetFromJSONFileAsync<Config>("config.json", Environment.CurrentDirectory);
        }
    }
}
