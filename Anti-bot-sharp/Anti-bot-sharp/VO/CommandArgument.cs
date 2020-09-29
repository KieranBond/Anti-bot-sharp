using Discord.WebSocket;

namespace AntiBotSharp.VO
{
    public class CommandArgument
    {
        public string Argument { get; private set; }

        public bool IsUserMention { get; private set; }
        public bool IsChannelMention { get; private set; }

        public SocketUser MentionedUser { get; private set; }
        public SocketChannel MentionedChannel { get; private set; }

        public CommandArgument(string argument, SocketUser mentionedUser = null, SocketChannel mentionedChannel = null)
        {
            Argument = argument;

            if(mentionedUser != null)
            {
                IsUserMention = true;
                MentionedUser = mentionedUser;
            }
            else if(mentionedChannel != null)
            {
                IsChannelMention = true;
                MentionedChannel = mentionedChannel;
            }
        }
    }
}
