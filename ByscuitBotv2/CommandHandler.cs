using ByscuitBotv2;
using ByscuitBotv2.Byscoin;
using ByscuitBotv2.Commands;
using ByscuitBotv2.Data;
using ByscuitBotv2.Handler;
using ByscuitBotv2.Lotto;
using ByscuitBotv2.Modules;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        public static SocketRole PremiumByscuitRole = null;
        public static SocketRole AIUserRole = null;
        public ulong UnbakedByscuitID = 1046926179552219226;
        public static SocketRole UnbakedByscuitRole = null; // 1046926179552219226
        public static SocketGuild Byscuits = null;
        Random random = new Random();

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
            this.client.LatencyUpdated += Client_LatencyUpdated;

            this.client.Connected += Client_Connected;
            this.client.Ready += Client_Ready;
            this.client.LoggedOut += Client_LoggedOut;
            this.client.LoggedIn += Client_LoggedIn;
            this.client.Disconnected += Client_Disconnected;
            this.client.ReactionAdded += Client_ReactionAdded;
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
        {
            if (arg3.User.Value.IsBot) return;
            Console.WriteLine($"{arg3.User} reacted with {arg3.Emote.Name}");
            Console.WriteLine($"arg1: {arg1.Id}");
            Console.WriteLine($"arg2: {arg2.Id}");
            Console.WriteLine($"VCKick.DirectMessages: {VCKick.DirectMessages.Length}");
            Console.WriteLine($"VCKick.VotedMessages: {VCKick.VotedMessages.Count}");
            bool isDMInList = VCKick.CheckMsgInVote(arg1.Id);
            bool isVoted = VCKick.CheckMsgAlreadyVoted(arg1.Id);

            Console.WriteLine($"isDMInList: {isDMInList}");
            Console.WriteLine($"isVoted: {isVoted}");
            if (isDMInList && !isVoted) {
                RestUserMessage message = await arg1.GetOrDownloadAsync() as RestUserMessage;
                await message.ModifyAsync(m =>
                {
                    var OldEmbed = m.Embed.GetValueOrDefault();
                    EmbedBuilder embed = new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithTitle(OldEmbed.Title)
                        .WithDescription($"You Voted {arg3.Emote}")
                        .WithCurrentTimestamp();
                    m.Embed = embed.Build();
                });
                VCKick.VotedMessages.Add(arg1.Value);
                VCKick.ProcessVote(arg3.Emote);
            }
            await Task.CompletedTask;
        }

        int postureTime = DateTime.Now.Hour / 2;
        RestUserMessage sentMessage = null;
        int count = 0;
        public static DateTime WS_UPDATED_DATE = DateTime.Now;
        private Task Client_LatencyUpdated(int arg1, int arg2)
        {
            // Check if Vote in progress
            if (PermComs.VOTE_IN_PROGRESS)
            {
                if(VCKick.Expiration <= DateTimeOffset.Now) PermComs.VOTE_IN_PROGRESS=false;
            }
            /*
            // Posture Check
            SocketGuild Byscuits = client.GetGuild(246718514214338560); // Da Byscuits
            if (Byscuits == null) return Task.CompletedTask;
            SocketGuildUser colin = Byscuits.GetUser(325858971925610497) as SocketGuildUser; // Been_Loadin
            if (colin != null)
            {
                if (colin.VoiceChannel != null && colin.VoiceChannel != Byscuits.AFKChannel)
                {
                    printConsole("Posture Time: " + postureTime);
                    if (colin.VoiceChannel.Users.Count > 2)
                    {
                        if (DateTime.Now.Hour / 2 != postureTime)
                        {
                            postureTime = DateTime.Now.Hour / 2;

                            // Get all the members in the voice channel
                            SocketVoiceChannel vChan = colin.VoiceChannel;
                            string msg = "`POSTURE CHECK` \n";
                            foreach (SocketGuildUser user in vChan.Users) { if (user.IsBot) continue; msg += " " + user.Mention; }

                            sentMessage = colin.VoiceChannel.Guild.DefaultChannel.SendMessageAsync(msg).Result;
                        }
                    }
                }
            }
            // Delete posture check message
            if (sentMessage != null) { if (count++ > 2) { sentMessage.DeleteAsync().GetAwaiter(); sentMessage = null; count = 0; } }
            */
            // Check if any new deposits are coming in
            // Might want to create a new thread for this
            if (Deposit.NEW_DEPOSITS) {
                printDEBUG($"{Deposit.depositClaims.Count} Byscoin deposit claims...");
                Deposit.CheckDepositClaims();
            }

            // WorkerStates Update
            if(WS_UPDATED_DATE.AddHours(5) < DateTime.Now) WorkerStates.UpdateStates();
            return Task.CompletedTask;
        }

        private Task Client_GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser arg2)
        {
            printDEBUG($"GuildMember Updated | ARG 1: {arg1.Value} | ARG 2: {arg2}");
            
            return Task.CompletedTask;
        }
        

        private Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState vState1, SocketVoiceState vState2)
        {
            if (user.IsBot) return Task.CompletedTask;
            // Get current guild
            SocketGuild guild = null;
            if (vState1.VoiceChannel != null) guild = vState1.VoiceChannel.Guild;
            else if (vState2.VoiceChannel != null) guild = vState2.VoiceChannel.Guild;

            // Should I check if the user is alone?

            if (vState1.VoiceChannel != vState2.VoiceChannel)
            {
                if (vState2.VoiceChannel != null)// If user joins a channel
                {
                    // Mute check
                    if (!vState2.IsMuted && !vState2.IsSelfMuted) Accounts.UpdateUser(user.Id, true, true);//Start counting if unmuted
                    else if (vState2.IsMuted || vState2.IsSelfMuted) Accounts.UpdateUser(user.Id, false, true);// Stop counting if muted
                    printDEBUG($"{user} muted: {(vState2.IsMuted || vState2.IsSelfMuted)}");
                    printDEBUG($"{user} Channel joined: {guild.Name}/{vState2.VoiceChannel}");
                }
                else if (vState2.VoiceChannel == null)// If user leaves the channel
                {
                    Accounts.UpdateUser(user.Id, false);// Stop the counting
                    printDEBUG($"{user} Channel left: {guild.Name}/{vState1.VoiceChannel}");
                }
            }
            else// If the user deafens/mutes but doesnt change channels
            {   // Mute check
                if (vState2.IsMuted || vState2.IsSelfMuted) Accounts.UpdateUser(user.Id, false);// Stop counting if muted
                else Accounts.UpdateUser(user.Id, true, true); //Start counting if unmuted
                printDEBUG($"{user} muted: {(vState2.IsMuted || vState2.IsSelfMuted)}");
            }

            if (guild == null) return Task.CompletedTask;

            if(vState2.VoiceChannel == guild.AFKChannel)
            {
                Accounts.UpdateUser(user.Id, false);//Stop counting if AFK
                printDEBUG($"{user} is in AFK Channel");
            }

            foreach (Accounts.Account VCAccount in Accounts.GetAccountsInVC())
            {
                if (guild.GetUser(VCAccount.DiscordID).VoiceChannel == null)
                {
                    Accounts.UpdateUser(VCAccount.DiscordID, false);
                    printDEBUG($"{guild.GetUser(VCAccount.DiscordID)} is no longer in a channel");
                }
            }

            // ---- FIX THIS ----
            // Update user roles
            Accounts.Account account = Accounts.GetUser(user.Id);
            List<Roles.Role> roles = Roles.CheckRoles(account.GetHours());
            bool newRoles = false;
            string strRoles = "";
            SocketGuildUser sUser = user as SocketGuildUser;
            for (int i = 0; i < roles.Count; i++)
            {
                foreach (SocketRole sRole in guild.Roles)
                {
                    if (sRole.Id == roles[i].RoleID)
                    {
                        if (!sUser.Roles.Contains(sRole))
                        {
                            newRoles = true;
                            sUser.AddRoleAsync(sRole);
                            if (!String.IsNullOrEmpty(strRoles)) strRoles = $"[{sRole.Name}]";
                            else strRoles += $" [{sRole.Name}]";
                            continue;
                        }
                    }
                }
            }
            printDEBUG($"Roles: {strRoles}");
            //if (newRoles) guild.DefaultChannel.SendMessageAsync($"> **{user}**_({user.Id})_ has earned **{strRoles}**!");

            return Task.CompletedTask;
        }
       
        private async Task printAudit(SocketGuild guild, SocketUser user)
        {
            if (user == null || guild == null) await Task.CompletedTask;
            SocketTextChannel sChannel = GetTextChannel("security", guild);
            if (sChannel == null) await Task.CompletedTask;
            List<IReadOnlyCollection<RestAuditLogEntry>> aLog = await guild.GetAuditLogsAsync(1).ToListAsync();

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            bool print = false;
            foreach (RestAuditLogEntry audit in aLog[0])
            {
                if(audit.Action == ActionType.Ban)
                {
                    embed.WithColor(new Color(255, 0, 0));

                }
                else if (audit.Action == ActionType.MemberUpdated)
                {
                    string mod = $"**{audit.User.ToString()}**_({audit.User.Id})_";
                    embed.WithAuthor("Server Report", audit.User.GetAvatarUrl());
                    string reason = audit.Reason;
                    MemberUpdateAuditLogData data = (MemberUpdateAuditLogData)audit.Data;
                    string muted = "";
                    string deaf = "";
                    if (data.Before.Mute.HasValue || data.After.Mute.HasValue || data.Before.Deaf.HasValue || data.After.Deaf.HasValue)
                    {
                        if (data.Before.Mute.Value == data.After.Mute.Value && data.Before.Deaf.Value == data.After.Deaf.Value) return;

                        if (data.Before.Mute.Value != data.After.Mute.Value)
                        {
                            if (!data.Before.Mute.Value && data.After.Mute.Value) muted = "muted ";
                            else if (data.Before.Mute.Value && !data.After.Mute.Value) muted = "unmuted ";
                            print = true;
                        }
                        if (data.Before.Deaf.Value != data.After.Deaf.Value)
                        {
                            if (!data.Before.Deaf.Value && data.After.Deaf.Value) deaf = "deafened ";
                            else if (data.Before.Deaf.Value && !data.After.Deaf.Value) deaf = "undeafened ";
                            print = true;
                        }
                        if (muted != "" && deaf != "") muted += "& ";
                        string msg = $"**{data.Target}**_({data.Target.Id})_ was {muted}{deaf}by {mod}";
                        embed.WithColor(new Color(250, 150, 0));
                        embed.Description = msg;
                        embed.WithTimestamp(audit.CreatedAt);
                        embed.WithFooter(audit.Id.ToString());
                    }
                }
            } 
            printDEBUG(embed.Description);
            if (print) await sChannel.SendMessageAsync("", false, embed.Build());

        }

        private Task Client_UserUnbanned(SocketUser user, SocketGuild guild)
        {
            // Read audit log and tell who unbans another user
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = "Server Report";
            embed.WithAuthor("Server Report", user.GetAvatarUrl());
            return Task.CompletedTask;
        }

        private Task Client_UserBanned(SocketUser user, SocketGuild guild)
        {
            // Read audit log and tell who bans another user
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = "Server Report";
            embed.WithAuthor("Server Report", user.GetAvatarUrl());
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
            ulong AIUserID = 1047742786990002256;
            printConsole("Ready for inputs!");
            user = client.CurrentUser.ToString();
            Accounts.Load();
            Roles.Load();
            Byscuits = client.GetGuild(246718514214338560); // Da Byscuits
            PremiumByscuitRole = Byscuits.GetRole(765403412568735765); // Premium Byscuit role 765403412568735765
            AIUserRole = Byscuits.GetRole(AIUserID);
            UnbakedByscuitRole = Byscuits.GetRole(UnbakedByscuitID);
            CreditsSystem.LoadAccounts(Byscuits);
            BinanceWallet.Load();
            CashoutSystem.Load();
            LottoSystem.Load();
            Deposit.Load();
            // Nanopool stuff
            /*
            WorkerStates.Load();
            WorkerStates.UpdateStates();
            Nanopool.payoutThreshold = Program.config.NANOPOOL_PAYOUT;
            */
            Utility.SetDebugLevel(Program.config.DEBUG_LEVEL);
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
                printConsole($"Connected to discord as {user}");
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
            bool isDM = context.IsPrivate;

            if (isDM) { HandlePrivateMessage(context); return; }

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

                      var embed = new EmbedBuilder();
                    embed.WithTitle("Command Error");
                    embed.WithDescription(result.ErrorReason);
                    embed.WithColor(255, 0, 0);
                    embed.WithFooter("View console for more info");
                    embed.WithCurrentTimestamp();

                    await context.Channel.SendMessageAsync(embed: embed.Build());
                    printERROR(result.ErrorReason);
                    printERROR(result.Error);
                }
            }
            else // Non-Command messages
            {
            }


            await checkStats(context.Guild); // Check the stats
        }

        /// <summary>
        /// Sends message when user leaves and checks stats category
        /// </summary>
        /// <param name="user">Current User</param>
        /// <returns></returns>
        private async Task Client_UserLeft(SocketGuild guild, SocketUser user)
        {
            string username = user.ToString();
            var channel = guild.SystemChannel;
            string msg = String.Format("**{0}** has left the server...", username);  //Bye Message
            if (user.Id != BotID)
                await channel.SendMessageAsync(msg);   //Welcomes the new user


            // Read audit log and tell who kicked another user if they did
            /*
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = "Server Report";
            embed.WithAuthor("Server Report", user.GetAvatarUrl());
            */

            printConsole(username + " left " + guild.Name);
            await checkStats(guild); // Update user count
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

            // Add user to the byscoin accounts
            CreditsSystem.AddUser(user);

            // Read audit log and tell who invited another user
            /*
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = "Server Report";
            embed.WithAuthor("Server Report", user.GetAvatarUrl());
            */
            await user.AddRoleAsync(UnbakedByscuitRole);
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
                int bots = int.Parse(botChan.Name.Split(':')[1]);
                if (members != memberCount || botcount != bots)
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
        public SocketTextChannel GetTextChannel(string name, SocketGuild guild)
        {
            IReadOnlyCollection<SocketTextChannel> sChannels = guild.TextChannels;
            foreach (SocketTextChannel chan in sChannels)
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

        #region Console Printing Functions
        /// <summary>
        /// Print text to the console with timestamp.
        /// </summary>
        /// <param name="message">The message to print.</param>
        public void printConsole(string message)
        {
            Utility.printConsole(message);
        }
        /// <summary>
        /// Print object to the console with timestamp.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        public void printConsole(object obj)
        {
            Utility.printConsole(obj);
        }

        /// <summary>
        /// Print text to the console as an error with timestamp.
        /// </summary>
        /// <param name="message">The message to print.</param>
        public void printERROR(string message)
        {
            Utility.printERROR(message);
        }
        /// <summary>
        /// Print object to the console as an error with timestamp.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        public void printERROR(object obj)
        {
            Utility.printERROR(obj);
        }

        /// <summary>
        /// Print text to the console with the debug tag and timestamp.
        /// </summary>
        /// <param name="message">The message to print.</param>
        public void printDEBUG(string message)
        {
            Utility.printDEBUG(message);
        }
        /// <summary>
        /// Print object to the console with the debug tag and timestamp.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        public void printDEBUG(object obj)
        {
            Utility.printDEBUG(obj);
        }

        /// <summary>
        /// Print text to the console with the log tag and timestamp.
        /// </summary>
        /// <param name="message">The message to print.</param>
        public void printLOG(string message)
        {
            Utility.printLOG(message);
        }
        /// <summary>
        /// Print object to the console with the log tag and timestamp.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        public void printLOG(object obj)
        {
            Utility.printLOG(obj);
        }
        #endregion

        #region Message Handlers
        private void HandlePrivateMessage(SocketCommandContext context)
        {
            // Do stuff
        }
        #endregion
    }
}
