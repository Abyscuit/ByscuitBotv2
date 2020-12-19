using ByscuitBotv2.Modules;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace byscuitBot
{
    public class CommandHandler
    {
        DiscordSocketClient client;
        static CommandService service;
        public bool bBotID = false;
        public static ulong BotID = 510066148285349900;// Placeholder but is dynamically set upon initialization
        public List<IMessage> oldMessage = new List<IMessage>();
        public static string prefix = "/";
        bool disconnected = false;
        string user = "";

        /*
         * ------------------------------------------- *
         *  Create credit system with daily check in   *
         *  rewards. Add gambling games for credits.   *
         *  Add timer for gambling games? learn how to *
         *  manage async threads better.               *
         *  Add notification of member join vc, user   *
         *  disconnects, bans, kicks another. Spam     *
         *  detection                                  *
         * ------------------------------------------- *
         */
        public async Task InitializeAsync(DiscordSocketClient client)
        {
            this.client = client;
            service = new CommandService();
            await service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            this.client.MessageReceived += Client_MessageReceived;
            this.client.UserJoined += Client_UserJoined;
            this.client.UserLeft += Client_UserLeft;
            this.client.UserBanned += Client_UserBanned;
            this.client.UserUnbanned += Client_UserUnbanned;
            this.client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
            this.client.GuildMemberUpdated += Client_GuildMemberUpdated;

            this.client.Connected += Client_Connected;
            this.client.Ready += Client_Ready;
            this.client.LoggedOut += Client_LoggedOut;
            this.client.LoggedIn += Client_LoggedIn;
            this.client.Disconnected += Client_Disconnected;
        }

        private Task Client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            return Task.CompletedTask;
        }

        private Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            return Task.CompletedTask;
        }

        private Task Client_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            return Task.CompletedTask;
        }

        private Task Client_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            return Task.CompletedTask;
        }

        private Task Client_LoggedIn()
        {
            printConsole(user + " has been logged in!");
            return Task.CompletedTask;
        }

        private Task Client_LoggedOut()
        {
            printConsole(user + " has been logged out...");
            user = "";
            return Task.CompletedTask;
        }

        private Task Client_Ready()
        {
            printConsole("Ready for inputs!");
            user = client.CurrentUser.ToString();
            return Task.CompletedTask;
        }

        private Task Client_Disconnected(Exception arg)
        {
            printConsole("Disconnected from discord...");
            printConsole("Attempting to reconnect");
            disconnected = true;
            return Task.CompletedTask;
        }

        private Task Client_Connected()
        {
            user = client.CurrentUser.ToString();
            if (!disconnected)
            {
                printConsole("Connected to discord");
                printConsole("User: " + user);
                IEnumerator<SocketGuild> lGuilds = client.Guilds.GetEnumerator();
                //string guilds = (lGuilds.MoveNext()) ? "" + lGuilds.Current.Id : "";
                
                
                //for (int i = 1; i < lGuilds.Count; i++) guilds += "/" + lGuilds[i].Name;
                //printConsole("Guilds: " + guilds);
            }
            else printConsole(user + " reconnected to discord");
            return Task.CompletedTask;
        }

        public static List<CommandInfo> GetCommands()
        {

            return service.Commands.ToList();
        }

        /// <summary>
        /// Checks the user input for commands and executes the command
        /// </summary>
        /// <param name="s">The message sent</param>
        /// <returns></returns>
        private async Task Client_MessageReceived(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            var context = new SocketCommandContext(client, msg);

            // Check if the bot ID is set to the correct ID
            if(!bBotID) BotID = client.CurrentUser.Id;

            SocketGuildUser user = (SocketGuildUser)context.User;
            SocketRole fByscuit = null;
            foreach(SocketRole role in context.Guild.Roles)
            {
                if(role.Name == "Fresh Byscuit")
                {
                    fByscuit = role;
                    break;
                }
            }
            if (fByscuit != null)
                if (!context.User.IsBot && !user.Roles.Contains(fByscuit) && DateTimeOffset.Now.Subtract(user.JoinedAt.Value) >= new TimeSpan(3,0,0,0)) await user.AddRoleAsync(fByscuit);

            int argPos = 0;
            //printConsole(context.Guild.Name);
            if (msg.HasStringPrefix(prefix, ref argPos)
                || msg.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                IResult result = null;
                result = await service.ExecuteAsync(context, argPos, null);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    string message = "```diff\n-Error when running this command\n-Reason: " + result.ErrorReason + "\n\n+View console for more info\n```";

                    /// No More embeds for right now
                    //  var embed = new EmbedBuilder();
                    //  embed.WithTitle("Command Error");
                    //  embed.WithDescription(message);
                    //  embed.WithColor(255, 120, 120);
                    //  embed.WithFooter("Developed by Abyscuit");
                    //  embed.WithCurrentTimestamp();

                    await context.Channel.SendMessageAsync(message);
                    Console.WriteLine(result.ErrorReason);
                    Console.WriteLine(result.Error);
                }
            }
            else // Non-Command messages
            {
                /*//  Code just for mentions
                IReadOnlyCollection<SocketUser> socketUsers = context.Message.MentionedUsers;
                IEnumerator<SocketUser> users = socketUsers.GetEnumerator();
                while (users.MoveNext())
                {
                    printConsole(context.User + " mentioned " + users.Current);
                    if (users.Current.Id == 222866066961989632 && context.Guild.Id != 246718514214338560)
                    {
                        await context.Channel.SendMessageAsync("Fuck off yo! Message me in Da Byscuits server");
                        break;
                    }
                }
                */
            }


            await checkStats(context.Guild); // Check the stats
        }

        /// <summary>
        /// Sends message when user leaves and checks stats category
        /// </summary>
        /// <param name="user">Current User</param>
        /// <returns></returns>
        private async Task Client_UserLeft(SocketGuildUser user)
        {
            string username = user.ToString();
            var channel = user.Guild.SystemChannel;
            string msg = String.Format("**{0}** has left the server...", username);  //Bye Message
            if (user.Id != BotID)
                await channel.SendMessageAsync(msg);   //Welcomes the new user

            printConsole(username + " left " + user.Guild.Name);
            await checkStats(user.Guild); // Update user count
        }

        /// <summary>
        /// Sends message when user joins and checks stats category
        /// </summary>
        /// <param name="user">Current user</param>
        /// <returns></returns>
        private async Task Client_UserJoined(SocketGuildUser user)
        {
            string username = user.ToString();
            var channel = user.Guild.SystemChannel;
            string msg = String.Format("**{0}** has joined the server!", username);  //Welcome Message
            if (user.Id != BotID)
                await channel.SendMessageAsync(msg);   //Welcomes the new user

            printConsole(username + " joined " + user.Guild.Name);
            await checkStats(user.Guild); // Update user count
        }


        public async Task checkStats(SocketGuild guild)
        {
            IReadOnlyCollection<SocketGuildUser> users = guild.Users;

            int botcount = 0;
            foreach (SocketGuildUser user in users) if (user.IsBot) botcount++;

            int memberCount = users.Count;
            if (!guild.HasAllMembers) memberCount = guild.MemberCount;

            // Create a category for the server stats if it doesn't exist
            if (!CategoryExist(guild, "Stats"))
            {
                // Create the voice channels that report the stats
                IReadOnlyCollection<SocketCategoryChannel> categoryChannels = guild.CategoryChannels;
                RestCategoryChannel cat = await guild.CreateCategoryChannelAsync("Stats", m => { m.Position = 0;});

                // Create the permissions for the category
                OverwritePermissions catPerms = cat.GetPermissionOverwrite(guild.EveryoneRole).GetValueOrDefault();
                catPerms.ToDenyList().Add(ChannelPermission.Connect);// Add user connect to deny list
                await cat.AddPermissionOverwriteAsync(guild.EveryoneRole, catPerms);

                // If any of the channels don't exist create it
                if (GetVoiceChannel("Bot Count", guild) == null)
                {
                    RestVoiceChannel x = await guild.CreateVoiceChannelAsync("Bot Count: " + botcount);
                    await x.ModifyAsync(m => { m.Position = 1; m.UserLimit = 0; m.CategoryId = cat.Id; });
                    await x.SyncPermissionsAsync();
                }
                if (GetVoiceChannel("User Count", guild) == null)
                {
                    RestVoiceChannel z = await guild.CreateVoiceChannelAsync("User Count: " + (memberCount - botcount));
                    await z.ModifyAsync(m => { m.Position = 0; m.UserLimit = 0; m.CategoryId = cat.Id; });
                    await z.SyncPermissionsAsync();
                }
                if (GetVoiceChannel("Member Count", guild) == null)
                {
                    RestVoiceChannel z = await guild.CreateVoiceChannelAsync("Member Count: " + memberCount);
                    await z.ModifyAsync(m => { m.Position = 2; m.UserLimit = 0; m.CategoryId = cat.Id; });
                    await z.SyncPermissionsAsync();
                }
                await cat.ModifyAsync(m => m.Position = 0);// Push category back to top of the server menu
            }
            else
            {
                SocketVoiceChannel botChan = GetVoiceChannel("Bot Count", guild);
                SocketVoiceChannel userChan = GetVoiceChannel("User Count", guild);
                SocketVoiceChannel memberChan = GetVoiceChannel("Member Count", guild);
                int members = int.Parse(memberChan.Name.Split(':')[1]);
                if (members != memberCount)
                {
                    if (memberChan != null) await memberChan.ModifyAsync(m => m.Name = "Member Count: " + memberCount);
                    if (botChan != null && int.Parse(botChan.Name.Split(':')[1]) != botcount) await botChan.ModifyAsync(m => m.Name = "Bot Count: " + botcount);
                    int userVal = memberCount - botcount;
                    if (userChan != null && int.Parse(userChan.Name.Split(':')[1]) != userVal) await userChan.ModifyAsync(m => m.Name = "User Count: " + userVal);
                }
            }
        }

        public SocketVoiceChannel GetVoiceChannel(string name, SocketGuild guild)
        {
            IReadOnlyCollection<SocketVoiceChannel> sChannels = guild.VoiceChannels;
            foreach (SocketVoiceChannel chan in sChannels)
                if (chan.Name.ToLower().Contains(name.ToLower()))
                    return chan; // return the correct channel
            return null;// doesn't exist
        }

        public bool CategoryExist(SocketGuild guild, string name)
        {
            IReadOnlyCollection<SocketCategoryChannel> categoryChannels = guild.CategoryChannels;
            foreach(SocketCategoryChannel category in categoryChannels)
                if (category.Name.ToLower().Contains(name.ToLower())) return true;
            return false;
        }


        /// <summary>
        ///Print to console with the current timestamp
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public void printConsole(string message)
        {
            Console.WriteLine(DateTime.Now.ToLocalTime() + " | " + message);
        }
    }
}
