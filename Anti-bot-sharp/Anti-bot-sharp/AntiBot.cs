using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace AntiBotSharp
{
    public class AntiBot
    {
        private DiscordSocketClient _client;

        private string _clientToken;

        public AntiBot(string clientToken)
        {
            _clientToken = clientToken;

            _client = new DiscordSocketClient();
            _client.Log += (msg) => Log(msg.ToString());

            _client.Connected += OnConnected;
            _client.MessageReceived += OnMessageReceived;
        }

        private Task OnMessageReceived(SocketMessage message)
        {
            if(message.Author.IsBot)
            {

            }

            return Task.CompletedTask;
        }

        private Task OnConnected()
        {
            return Log("I am alive!");
        }

        public async Task Startup()
        {
            await _client.LoginAsync(TokenType.Bot, _clientToken);
            await _client.StartAsync();
        }

        private Task Log(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}
