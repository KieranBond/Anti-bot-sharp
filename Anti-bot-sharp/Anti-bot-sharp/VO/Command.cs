using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace AntiBotSharp.VO
{
    public class Command
    {
        public CommandType @CommandType { get; set; }
        public SocketMessage OriginalMessage { get; private set; }
        public SocketUser Author { get { return OriginalMessage.Author; } }

        public IReadOnlyList<CommandArgument> Arguments { get { return _arguments?.AsReadOnly(); } }
        private List<CommandArgument> _arguments;


        public Command(CommandType type, SocketMessage originalMessage, List<CommandArgument> arguments) : this(type, originalMessage)
        {
            _arguments = arguments;
        }

        public Command(CommandType type, SocketMessage originalMessage)
        {
            CommandType = type;
            OriginalMessage = originalMessage;
        }             
    }
}
