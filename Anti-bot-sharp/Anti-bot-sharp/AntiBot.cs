using AntiBotSharp.Helpers;
using AntiBotSharp.VO;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AntiBotSharp
{
    public class AntiBot
    {
        private const string _botPrefix = "?ybd";

        private DiscordSocketClient _client;

        private Config _config;
        private string _clientToken;

        private HashSet<SocketUser> _admins = new HashSet<SocketUser>();

        private HashSet<SocketUser> _blacklistedUsers = new HashSet<SocketUser>();
        private HashSet<string> _filteredWords = new HashSet<string>();
        private Dictionary<SocketUser, string> _timedOutUsers = new Dictionary<SocketUser, string>();

        public AntiBot(Config config)
        {
            Configure(config);

            _client = new DiscordSocketClient();
            _client.Log += (msg) => Log(msg.ToString());

            _client.Connected += OnConnected;
            _client.Ready += OnReady;
            _client.MessageReceived += OnMessageReceived;
        }

        private Task OnReady()
        {
            ConfigureAdmins();
            return Task.CompletedTask;
        }

        private void Configure(Config config)
        {
            _config = config;
            _clientToken = config.Token;
        }

        private async void ConfigureAdmins()
        {
            foreach(SocketGuild guild in _client.Guilds)
            {
                await Log("Looking for admins in guild: " + guild.Name);
                foreach(string admin in _config.Admins)
                {
                    var adminUser = guild.GetUser(ulong.Parse(admin));

                    if(adminUser != null)
                    {
                        await Log("Admin found: " + adminUser.Username);
                        _admins.Add(adminUser);
                    }
                }
            }
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            Command command = CommandParser.ParseMessage(_botPrefix, message);

            if (command != null)
            {
                //Only admins
                if(_admins.Contains(message.Author))
                    await HandleCommand(command);
            }
            else if(IsMessageAuthorBlacklisted(message.Author))
            {
                await HandleBlacklistedAuthor(message);
            }
            else if(IsMessageFiltered(message.Content))
            {
                await HandleFilteredMessage(message);
            }
            else if(IsMessageAuthorOnTimeout(message.Author))
            {
                await HandleTimeoutUser(message);
            }
        }

        private bool IsMessageAuthorOnTimeout(SocketUser author)
        {
            return _timedOutUsers.ContainsKey(author);
        }

        private async Task HandleTimeoutUser(SocketMessage message)
        {
            await message.DeleteAsync();

            int timeRemaining = Timewatch.GetRemainingTime(_timedOutUsers[message.Author]);
            await message.Author.SendMessageAsync(string.Format("You're on a timeout for {0} more seconds.", timeRemaining));
        }

        private async Task HandleBlacklistedAuthor(SocketMessage message)
        {
            await message.DeleteAsync();
            await message.Author.SendMessageAsync("You've been blacklisted.");
        }

        private async Task HandleFilteredMessage(SocketMessage message)
        {
            int numberOfBadWords = GetFilteredWordsFromMessage(message.Content).Count;

            await message.DeleteAsync();

            if(numberOfBadWords > 1)
                await message.Channel.SendMessageAsync("Stop using bad words.");
        }

        private async Task HandleCommand(Command command)
        {
            switch(command.CommandType)
            {
                case CommandType.AddAdmin:

                    foreach(CommandArgument argument in command.Arguments)
                    {
                        if(argument.IsUserMention)
                        {
                            SocketUser userToAdd = argument.MentionedUser;
                            await Log("Adding admin: " + userToAdd.Username);
                            _admins.Add(userToAdd);
                        }
                    }

                    break;

                case CommandType.RemoveAdmin:

                    foreach (CommandArgument argument in command.Arguments)
                    {
                        if (argument.IsUserMention)
                        {
                            SocketUser userToRemove = argument.MentionedUser;
                            await Log("Removing admin: " + userToRemove.Username);
                            _admins.Remove(userToRemove);
                        }
                    }

                    break;

                case CommandType.AddBlacklist:

                    foreach(CommandArgument argument in command.Arguments)
                    {
                        if (argument.IsUserMention)
                        {
                            _blacklistedUsers.Add(argument.MentionedUser);
                        }
                    }

                    break;

                case CommandType.RemoveBlacklist:

                    foreach (CommandArgument argument in command.Arguments)
                    {
                        if (argument.IsUserMention)
                        {
                            _blacklistedUsers.Remove(argument.MentionedUser);
                        }
                    }

                    break;

                case CommandType.AddFilter:

                    _filteredWords.Add(command.Arguments[0].Argument);

                    break;

                case CommandType.RemoveFilter:

                    _filteredWords.Remove(command.Arguments[0].Argument);

                    break;

                case CommandType.AddTimeout:

                    double timeoutDurationInSeconds;
                    if(double.TryParse(command.Arguments[1].Argument, out timeoutDurationInSeconds))
                    {
                        SocketUser mentionedUser = command.Arguments[0].MentionedUser;
                        if (mentionedUser == null)
                            return;

                        Action removeTimedOutUser = ()=> _timedOutUsers.Remove(mentionedUser);
                        string id = Timewatch.AddTimer(timeoutDurationInSeconds, removeTimedOutUser);

                        _timedOutUsers.Add(mentionedUser, id);

                        await Log(string.Format("Adding timeout of {0} seconds to {1}", timeoutDurationInSeconds, mentionedUser.Username));
                    }

                    break;

                case CommandType.RemoveTimeout:

                    if(command.Arguments[0].IsUserMention)
                    {
                        await Log("Removing timeout: " + command.Arguments[0].MentionedUser.Username);
                        Timewatch.RemoveTimer(_timedOutUsers[command.Arguments[0].MentionedUser]);
                        _timedOutUsers.Remove(command.Arguments[0].MentionedUser);
                    }

                    break;

                case CommandType.ListAdmins:

                    StringBuilder adminOutput = new StringBuilder("Admins:\n");
                    await Log("Admins:");
                    foreach (SocketUser admin in _admins)
                    {
                        await Log(admin.Username);
                        adminOutput.Append(admin.Username).Append("\n");
                    }

                    await command.OriginalMessage.Channel.SendMessageAsync(adminOutput.ToString());

                    break;

                case CommandType.ListBlacklist:

                    StringBuilder blacklistedUsersOutput = new StringBuilder("Blacklist:\n");

                    await Log("Blacklist:");
                    foreach (SocketUser blacklistedUser in _blacklistedUsers)
                    {
                        string output = string.Format("Name: {0}, ID: {1}", blacklistedUser.Username, blacklistedUser.Id);
                        await Log(output);
                        blacklistedUsersOutput.Append(output).Append("\n");
                    }

                    await command.OriginalMessage.Channel.SendMessageAsync(blacklistedUsersOutput.ToString());

                    break;

                case CommandType.ListFilter:

                    StringBuilder filteredWordsOutput = new StringBuilder("Filtered Words:\n");

                    await Log("Filtered words:");
                    foreach (string filteredWord in _filteredWords)
                    {
                        filteredWordsOutput.Append(filteredWord).Append("\n");
                        await Log(filteredWord);
                    }

                    await command.OriginalMessage.Channel.SendMessageAsync(filteredWordsOutput.ToString());

                    break;

                case CommandType.GetID:

                    await Log("Author ID: " + command.Author.Id);

                    await Log("ID's in order of @:");

                    if (command.Arguments == null || command.Arguments.Count == 0)
                        return;

                    foreach(CommandArgument arg in command.Arguments)
                    {
                        if(arg.MentionedUser != null)
                            await Log("ID: " + arg.MentionedUser.Id);
                    }

                    break;

                case CommandType.Cleanup:

                    var channel = command.OriginalMessage.Channel;
                    var messages = await channel.GetMessagesAsync().FlattenAsync();
                    foreach(IMessage messageInChannel in messages)
                    {
                        if(messageInChannel.Content.StartsWith(_botPrefix))
                        {
                            await messageInChannel.DeleteAsync();
                        }
                    }

                    await Log("Finished cleanup");
                    await channel.SendMessageAsync("Cleanup complete.");

                    break;

                case CommandType.Help:

                    LogHelp(command.OriginalMessage.Channel);

                    break;

                case CommandType.None:

                    return;

                    break;
            }
        }

        private bool IsMessageAuthorBlacklisted(SocketUser author)
        {
            return _blacklistedUsers.Contains(author);
        }

        private bool IsMessageFiltered(string message)
        {
            string[] messageWords = message.ToLower().Split(" ");

            bool wordInMessageIsFiltered = false;

            foreach(string word in messageWords)
            {
                if (_filteredWords.Contains(word))
                    wordInMessageIsFiltered = true;
            }

            return wordInMessageIsFiltered;
        }

        private List<string> GetFilteredWordsFromMessage(string message)
        {
            string[] words = message.ToLower().Split(" ");

            List<string> filteredWords = new List<string>();

            foreach (string word in words)
            {
                foreach (string filteredWord in _filteredWords)
                {
                    if (filteredWord.Contains(word))
                        filteredWords.Add(word);
                }
            }

            return filteredWords;
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

        private async void LogHelp(ISocketMessageChannel destinationChannel)
        {
            string helpString = new StringBuilder()
                    .Append("YourBigDaddy is always here to help.\n")
                    .Append("\n")
                    .Append("Prefix: `?ybd`.\n")
                    .Append("All commands are case insensitive, and require Admin power.\n")
                    .Append("\n")
                    .Append("`addadmin @` : Everybody mentioned gets made an Admin.\n")
                    .Append("`removeadmin @` : Opposite of add.\n")
                    .Append("`listadmins` lists all admins in channel and console.\n")
                    .Append("\n")
                    .Append("`addblacklist @` basically global mute on whoever is mentioned.\n")
                    .Append("`removeblacklist @` removes any mentioned from the global mute\n")
                    .Append("`listblacklist` lists everybody who's blacklisted in channel and console.\n")
                    .Append("\n")
                    .Append("`addfilter filteredwordhere` word entered as argument is in filter list. If somebody uses it, it gets deleted.\n")
                    .Append("`removefilter filteredwordhere` opposite of add. Removes from list.\n")
                    .Append("`listfilter` lists all filtered words in the console and channel.\n")
                    .Append("\n")
                    .Append("`addtimeout @ x` gives user mentioned a temporary mute for x seconds.\n")
                    .Append("`removetimeout @` removes any timeout on mentioned user.\n")
                    .Append("\n")
                    .Append("`cleanup` simply removes the last 100 messages using the prefix. Can take a while so be sparing with this. It will output when done.\n")
                    .Append("\n")
                    .Append("[DEBUG]\n")
                    .Append("`getid @` logs to console bot is running on the ID of message Author and anybody mentioned.").ToString();
            
            await Log(helpString);
            await destinationChannel.SendMessageAsync(helpString);
        }

    }
}
