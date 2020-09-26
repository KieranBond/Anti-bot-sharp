using AntiBotSharp.Helpers;
using AntiBotSharp.VO;
using System.Threading.Tasks;

namespace AntiBotSharp
{
    public class Program
    {
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            Config cfg = await Config.BuildConfig();
            string clientToken = cfg.Token;

            AntiBot bot = new AntiBot(cfg);
            await bot.Startup();


            //Wait until program closed.
            await Task.Delay(-1);
        }
    }
}
