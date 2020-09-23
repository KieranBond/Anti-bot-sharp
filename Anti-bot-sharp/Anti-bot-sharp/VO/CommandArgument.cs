using Discord.WebSocket;

namespace AntiBotSharp.VO
{
    public class CommandArgument
    {
        public string Argument { get; private set; }

        public bool IsUserMention { get; private set; }

        public SocketUser MentionedUser { get; private set; }

        public CommandArgument(string argument, SocketUser mentionedUser = null)
        {
            Argument = argument;

            if(mentionedUser != null)
            {
                IsUserMention = true;
                MentionedUser = mentionedUser;
            }
        }
    }
}
