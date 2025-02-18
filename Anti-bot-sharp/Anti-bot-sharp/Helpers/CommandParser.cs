﻿using AntiBotSharp.VO;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiBotSharp.Helpers
{
    public static class CommandParser
    {
        public static Command ParseMessage(string prefix, SocketMessage message)
        {
            if (!message.Content.StartsWith(prefix))
                return null;

            string[] props = message.Content.Split(" ");

            //Only contains prefix, no point continuing
            if (props.Length <= 1)
                return null;

            string command = props[1].ToLower();

            var commandType = DetermineCommand(command);

            List<CommandArgument> arguments = new List<CommandArgument>();

            //1st prop is prefix, 2nd is command, 3rd+ is arguments
            if (props.Length >= 3)
            {
                for (int i = 2; i < props.Length; i++)
                {
                    string property = props[i];
                    var mentionedUser = GetMentionedUser(property, message.MentionedUsers);
                    var mentionedChannel = GetMentionedChannel(property, message.MentionedChannels);
                    var mentionedRole = GetMentionedRole(property, message.MentionedRoles);

                    CommandArgument argument = new CommandArgument(property, mentionedUser, mentionedChannel, mentionedRole);
                    arguments.Add(argument);
                }
            }

            Command parsedCommand;

            if (arguments.Count > 0)
                parsedCommand = new Command(commandType, message, arguments);
            else
                parsedCommand = new Command(commandType, message);

            return parsedCommand;
        }

        private static SocketRole GetMentionedRole(string property, IReadOnlyCollection<SocketRole> mentionedRoles)
        {
            Console.WriteLine("Mentioned role? " + property);

            if(mentionedRoles.Count > 0)
            {
                if (mentionedRoles.Count == 1)
                {
                    return mentionedRoles.ToList()[0];
                }

                foreach(SocketRole mentionedRole in mentionedRoles)
                {
                    if(property.Contains(mentionedRole.Id.ToString()))
                    {
                        return mentionedRole;
                    }
                }
            }

            return null;
        }

        private static SocketChannel GetMentionedChannel(string property, IEnumerable<SocketGuildChannel> mentionedChannels)
        {
            Console.WriteLine("Mentioned channel? " + property);

            if(property.StartsWith("<#") && property.EndsWith(">"))
            {
                string strippedChannelID = property.Replace("<#", "").Replace(">", "");

                foreach(SocketChannel mentionedChannel in mentionedChannels)
                {
                    if(mentionedChannel.Id.ToString() == strippedChannelID)
                    {
                        return mentionedChannel;
                    }
                }
            }

            return null;
        }

        private static SocketUser GetMentionedUser(string property, IEnumerable<SocketUser> mentionedUsers)
        {
            if (property.StartsWith("<@!") && property.EndsWith(">"))
            {
                string strippedID = property.Replace("<@!", "").Replace(">", "");

                foreach(SocketUser mentioned in mentionedUsers)
                {
                    if (mentioned.Id.ToString() == strippedID)
                        return mentioned;
                }
            }

            return null;
        }

        private static CommandType DetermineCommand(string command)
        {
            switch (command)
            {
                case "addadmin":

                    return CommandType.AddAdmin;

                    break;

                case "removeadmin":

                    return CommandType.RemoveAdmin;

                    break;
                case "addblacklist":

                    return CommandType.AddBlacklist;

                    break;

                case "removeblacklist":

                    return CommandType.RemoveBlacklist;

                    break;

                case "addfilter":

                    return CommandType.AddFilter;

                    break;

                case "removefilter":

                    return CommandType.RemoveFilter;

                    break;

                case "addtimeout":

                    return CommandType.AddTimeout;

                    break;

                case "removetimeout":

                    return CommandType.RemoveTimeout;

                    break;

                case "settimeoutrole":

                    return CommandType.SetTimeoutRole;

                    break;

                case "listblacklist":

                    return CommandType.ListBlacklist;

                    break;

                case "listfilter":

                    return CommandType.ListFilter;

                    break;

                case "listadmins":

                    return CommandType.ListAdmins;

                    break;

                case "cleanup":

                    return CommandType.Cleanup;

                    break;

                case "auditlogtarget":

                    return CommandType.AuditLogTarget;

                    break;

                case "toggleauditlog":

                    return CommandType.ToggleAuditLog;

                    break;

                case "getid":

                    return CommandType.GetID;

                    break;

                case "help":

                    return CommandType.Help;

                    break;

                default:

                    return CommandType.None;
            }
        }
    }
}