﻿using ByscuitBotv2.Byscoin;
using ByscuitBotv2.Data;
using ByscuitBotv2.Lotto;
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
        }
        int postureTime = DateTime.Now.Hour / 2;
        RestUserMessage sentMessage = null;
        int count = 0;
        int lottoDay = DateTime.Now.Day;
        public static List<SocketGuildUser> fullMatch = new List<SocketGuildUser>();
        public static List<SocketGuildUser> threeMatch = new List<SocketGuildUser>();
        public static List<SocketGuildUser> twoMatch = new List<SocketGuildUser>();
        private Task Client_LatencyUpdated(int arg1, int arg2)
        {
            
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
            printConsole("LottoDay: " + lottoDay);
            // Do byscoin Lotto
            if (DateTime.Now.Day != lottoDay)
            {
                // Generate 4 random numbers but randomize random
                random.Next(); random.Next(); random.Next(); random.Next();
                int[] winningNums = { random.Next(0, 9), random.Next(0, 9), random.Next(0, 9), random.Next(0, 9) };
                printConsole("Doing Byscoin Lotto");
                if (Accounts.accounts == null) return Task.CompletedTask;
                Accounts.Sort();
                twoMatch.Clear();
                threeMatch.Clear();
                fullMatch.Clear();
                // Add the top 10 leader board members
                for (int i = 0; i < LottoSystem.LOTTO_ENTRIES.Count; i++)
                {
                    LottoEntry entry = LottoSystem.LOTTO_ENTRIES[i];
                    int matchingNums = 0;
                    for (int j = 0; j < winningNums.Length; j++)
                        if (entry.numbers[j] == winningNums[j]) matchingNums++;
                    SocketGuildUser user = Byscuits.GetUser(entry.discordID);
                    if (matchingNums == 2) { if (!twoMatch.Contains(user)) twoMatch.Add(user); }
                    else if (matchingNums == 3) { if (!threeMatch.Contains(user)) threeMatch.Add(user); }
                    else if (matchingNums == 4) { if (!fullMatch.Contains(user)) fullMatch.Add(user); }
                }
                decimal TotalPot = LottoSystem.LOTTO_POT;
                if (twoMatch.Count > 0)
                {
                    // Give 10% pot / winners
                    decimal winnings = (LottoSystem.LOTTO_POT / 10) / twoMatch.Count;
                    LottoSystem.LOTTO_POT -= TotalPot / 10;
                    for(int i = 0; i < twoMatch.Count; i++) {
                        Account acc = CreditsSystem.GetAccount(twoMatch[i]);
                        acc.credits += (double)winnings;
                    }
                }
                if (threeMatch.Count > 0)
                {
                    // Give 25% pot / winners
                    decimal winnings = (TotalPot / 5) / threeMatch.Count;
                    LottoSystem.LOTTO_POT -= TotalPot / 5;
                    for (int i = 0; i < threeMatch.Count; i++)
                    {
                        Account acc = CreditsSystem.GetAccount(threeMatch[i]);
                        acc.credits += (double)winnings;
                    }
                }
                if (fullMatch.Count > 0)
                {
                    // Give 65-100% POT / winners
                    decimal winnings = (LottoSystem.LOTTO_POT / fullMatch.Count);
                    for (int i = 0; i < fullMatch.Count; i++)
                    {
                        Account acc = CreditsSystem.GetAccount(fullMatch[i]) ;
                        acc.credits += (double)winnings;
                    }
                }
                string winMsg = "> ";
                // Display message for two matching number winners
                for (int i = 0; i < twoMatch.Count; i++)
                {
                    if (i > 0) winMsg += " ";
                    winMsg += $"{twoMatch[i].Mention}";
                }
                string match2txt = "(matched with 2 numbers)";
                if (twoMatch.Count > 1) winMsg += $" split {TotalPot / 10} BYSC {match2txt}";
                else if (twoMatch.Count == 1) winMsg += $" won {TotalPot / 10} BYSC {match2txt}";

                // Display message for three matching number winners
                if (winMsg != "> " && threeMatch.Count > 0) winMsg += "\n> "; // Check if win message is empty
                string match3txt = "(matched with 3 numbers)";
                for (int i = 0; i < threeMatch.Count; i++)
                {
                    if (i > 0) winMsg += " ";
                    winMsg += $"{threeMatch[i].Mention}";
                }
                if (threeMatch.Count > 1) winMsg += $" split {TotalPot / 5} BYSC {match3txt}";
                else if (threeMatch.Count == 1) winMsg += $" won {TotalPot / 5} BYSC {match3txt}";

                // Display message for four matching number winners
                if (winMsg != "> " && fullMatch.Count > 0) winMsg += "\n> "; // Check if win message is empty
                string matchFulltxt = "(matched with 4 numbers)";
                for (int i = 0; i < fullMatch.Count; i++)
                {
                    if (i > 0) winMsg += " ";
                    winMsg += $"{fullMatch[i].Mention}";
                }
                if (fullMatch.Count > 1) winMsg += $" split {LottoSystem.LOTTO_POT} BYSC {matchFulltxt}";
                else if (fullMatch.Count == 1) winMsg += $" won {LottoSystem.LOTTO_POT} BYSC {matchFulltxt}";
                SocketTextChannel lottoChannel = Byscuits.GetTextChannel(840471064927666186);
                // Send the message if there are winners
                string msg = $"> WINNING NUMBERS: {winningNums[0]} {winningNums[1]} {winningNums[2]} {winningNums[3]}\n";
                if (twoMatch.Count > 0 || threeMatch.Count > 0 || fullMatch.Count > 0) lottoChannel.SendMessageAsync(msg + winMsg);
                else lottoChannel.SendMessageAsync(msg + "> No winners!");
                LottoSystem.LOTTO_ENTRIES.Clear();
                LottoSystem.LOTTO_POT = LottoSystem.INITIAL_LOTTO_POT;
                LottoSystem.Save();
                CreditsSystem.SaveFile();
                lottoDay = DateTime.Now.Day;
            }
            // Check if any new deposits are coming in
            // Might want to create a new thread for this
            if (Deposit.NEW_DEPOSITS) {
                printConsole($"{Deposit.depositClaims.Count} Byscoin deposit claims...");
                Deposit.CheckDepositClaims();
            }
            return Task.CompletedTask;
        }

        private Task Client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            printConsole($"GuildMember Updated | User 1: {arg1} | User 2: {arg2}");
            if(arg1.Id == 215535755727077379) // FubiRock nickname check
                if (arg2.Nickname != "FaggotRock") arg2.ModifyAsync(m => { m.Nickname = "FaggotRock"; }) ;
            
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
                    printConsole($"{user} muted: {(vState2.IsMuted || vState2.IsSelfMuted)}");
                    printConsole($"{user} Channel joined: {guild.Name}/{vState2.VoiceChannel}");
                }
                else if (vState2.VoiceChannel == null)// If user leaves the channel
                {
                    Accounts.UpdateUser(user.Id, false);// Stop the counting
                    printConsole($"{user} Channel left: {guild.Name}/{vState1.VoiceChannel}");
                }
            }
            else// If the user deafens/mutes but doesnt change channels
            {   // Mute check
                if (vState2.IsMuted || vState2.IsSelfMuted) Accounts.UpdateUser(user.Id, false);// Stop counting if muted
                else Accounts.UpdateUser(user.Id, true);//Start counting if unmuted
                printConsole($"{user} muted: {(vState2.IsMuted || vState2.IsSelfMuted)}");
            }

            if (guild == null) return Task.CompletedTask;

            if(vState2.VoiceChannel == guild.AFKChannel)
            {
                Accounts.UpdateUser(user.Id, false);//Stop counting if AFK
                printConsole($"{user} is in AFK Channel");
            }

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
            printConsole($"Roles: {strRoles}");
            if (newRoles) guild.DefaultChannel.SendMessageAsync($"> **{user}**_({user.Id})_ has earned **{strRoles}**!");

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
            printConsole(embed.Description);
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
            printConsole("Ready for inputs!");
            user = client.CurrentUser.ToString();
            Accounts.Load();
            Roles.Load();
            Byscuits = client.GetGuild(246718514214338560); // Da Byscuits
            PremiumByscuitRole = Byscuits.GetRole(765403412568735765); // Premium Byscuit role
            CreditsSystem.LoadAccounts(Byscuits);
            BinanceWallet.Load();
            CashoutSystem.Load();
            LottoSystem.Load();
            Deposit.Load();
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

                      var embed = new EmbedBuilder();
                    embed.WithTitle("Command Error");
                    embed.WithDescription(result.ErrorReason);
                    embed.WithColor(255, 0, 0);
                    embed.WithFooter("View console for more info");
                    embed.WithCurrentTimestamp();

                    await context.Channel.SendMessageAsync(embed: embed.Build());
                    Console.WriteLine(result.ErrorReason);
                    Console.WriteLine(result.Error);
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
        private async Task Client_UserLeft(SocketGuildUser user)
        {
            string username = user.ToString();
            var channel = user.Guild.SystemChannel;
            string msg = String.Format("**{0}** has left the server...", username);  //Bye Message
            if (user.Id != BotID)
                await channel.SendMessageAsync(msg);   //Welcomes the new user


            // Read audit log and tell who kicked another user if they did
            /*
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = "Server Report";
            embed.WithAuthor("Server Report", user.GetAvatarUrl());
            */

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

            // Add user to the byscoin accounts
            CreditsSystem.AddUser(user);

            // Read audit log and tell who invited another user
            /*
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = "Server Report";
            embed.WithAuthor("Server Report", user.GetAvatarUrl());
            */

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
