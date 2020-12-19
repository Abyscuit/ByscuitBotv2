using byscuitBot;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamKit2;
using System.Threading;
using System.Net.Http;

namespace ByscuitBotv2.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        string prefix = CommandHandler.prefix;

        [Command("help")]
        [Alias("help", "cmd", "cmds")]
        public async Task Help()
        {
            string title = "Use the '" + CommandHandler.prefix + "' prefix to send a command.\n";
            List<CommandInfo> cmds = CommandHandler.GetCommands();
            string userCmds = "";
            int x = 0;
            SocketGuildUser user = Context.User as SocketGuildUser;
            foreach (CommandInfo cmd in cmds)
            {
                if (cmd.Name.ToLower() == "help") continue;

                /// Conditionals for showing commands based
                /// on User privileges
                
                //  if (cmd.Preconditions.Count > 0)
                //  {
                //      userCmds += cmd.Name;
                //      if (!string.IsNullOrEmpty(cmd.Summary))
                //      userCmds += " - \"" + cmd.Summary + "\"\n\n";
                //      x++;
                //  }
                //  else
                //  {
                        userCmds += cmd.Name + " ";
                        foreach(string alias in cmd.Aliases)
                        userCmds += "[" + alias + "] ";
                        if (!string.IsNullOrEmpty(cmd.Summary))
                            userCmds += "\n\"" + cmd.Summary + "\"\n\n";
                        x++;// Add 1 to the command split counter
                //  }

                /// Split the message at the threshold
                //  if (x > 16)
                //  {
                //      userCmds += "|";
                //      x = 0;
                //  }
            }

            foreach (string s in SplitMessage(userCmds, '|'))
                if (!string.IsNullOrEmpty(s))
                    await Context.Channel.SendMessageAsync(string.Format(title + "{1}" + s + "{2}", prefix, "```ml\n", "\n```"));
        }

        [Command("Clear")]
        [Alias("Delete", "Clear", "dltmsg", "deletemsgs")]
        [Summary("Deletes a specified amount of messages in the channel - Usage: {0}clear <number>")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Clear(int num)
        {
            // Make sure number is over 0
            if (num <= 0) await Context.Channel.SendMessageAsync("Number to delete messages must be over 0...");

            await Context.Message.DeleteAsync();
            IAsyncEnumerable<IReadOnlyCollection<IMessage>> x = Context.Channel.GetMessagesAsync((num));// All the messages
            IAsyncEnumerator<IReadOnlyCollection<IMessage>> index = x.GetAsyncEnumerator();// Get message enumerator
            
            while (await index.MoveNextAsync())// Move to Next Message if exist
                foreach (IMessage msg in index.Current)// Delete message
                    await msg.DeleteAsync();
        }

        [Command("Inbox")]
        [Alias("mail", "byscuitmail", "bbmail")]
        [Summary("Check/create your ByscuitBros.site email - Usage: {0}inbox <num of messages>")]
        public async Task Inbox(int num = 0)
        {
            string username = Context.User.Username;
            if (num <= 0)
            {
                // Grab limit of inbox
                num = 20;
            }
            else
            {
                // Grab inbox amount
            }

            await Context.Channel.SendMessageAsync($"Grabbing {num} messages from inbox...");
            await Utility.DirectMessage(Context, $"Fetched {num} of messages from {username}@byscuitbros.site!");
        }

        static int taCount = 0;// Variable to keep track of throwaway
        [Command("ThrowAway")]
        [Alias("steamacct", "gensteam", "getsteam")]
        [Summary("Grab a throw-away steam account for CSGO - Usage: {0}throwaway <argument(optional)>")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task ThrowAway([Remainder]string text = "")
        {
            //if (!HasRole(Context.User as SocketGuildUser, "")) { return; }
            string path = "D:/Steam/Accounts.json";
            string strFile = File.ReadAllText(path);
            List<SteamAccount> steamAccounts = JsonConvert.DeserializeObject<List<SteamAccount>>(strFile);

            if (taCount >= steamAccounts.Count) taCount = 0;
            if (text == "")
            {
                await Context.Channel.SendMessageAsync("Sending login info...");
                SteamAccount acct = steamAccounts[taCount++];
                while (acct.CooldownEnd.CompareTo(DateTime.Now) > 0) { acct = steamAccounts[taCount++]; }
                string msg = string.Format("```ml\n{0} - \"Level {1}\"\nUSERNAME - '{2}'\nPASSWORD - '{3}'\n```", acct.Username, acct.Level, acct.Login, acct.Password);
                await Utility.DirectMessage(Context, msg);
            }
            else if (text == "list")
            {
                string msg = string.Format("```md\n{0} STEAM ACCOUNTS\n------------------", steamAccounts.Count);
                int cdCount = 0;
                int compCount = 0;
                int nnCount = 0;
                int utCount = 0;// Untested count
                int ut2Count = 0;// Untested level 2 count
                int splitCount = 0;
                foreach (SteamAccount acct in steamAccounts)
                {
                    /*
                    if (splitCount >= 40)
                    {
                        msg += "\n```";
                        await Context.Channel.SendMessageAsync(msg);
                        msg = "```md\n";
                        splitCount = 0;
                    }
                    */
                    if (acct.Level > 1) compCount++;
                    if (acct.Username.Contains("(UNTESTED)")) { utCount++; if (acct.Level > 1) ut2Count++; }
                    if (acct.Username.Contains("Unassigned Name")) { nnCount++; if (acct.CooldownEnd.CompareTo(DateTime.Now) < 0) continue; }
                    //msg += string.Format("\n[{0}](level {1})", acct.Username, acct.Level);
                    if (acct.CooldownEnd.CompareTo(DateTime.Now) > 0)
                    {
                        cdCount++;
                        //TimeSpan timeLeft = acct.CooldownEnd.Subtract(DateTime.Now);
                        //msg += string.Format("(cooldown ends in{2}{3}{4})", acct.Username, acct.Level, (timeLeft.Days <= 0) ? "" : $" {timeLeft.Days} days, ",
                        //    (timeLeft.Hours % 24 > 0) ? $" {timeLeft.Hours} hours," : "", (timeLeft.Minutes % 60 > 0) ? $" {timeLeft.Minutes} minutes" : "");
                    }
                    //splitCount++;
                }
                msg += string.Format("\n\n< {0} Accounts that are not named >", nnCount);
                msg += string.Format("\n# {0} Accounts with Cooldown", cdCount);
                msg += string.Format("\n# {0} Accounts that are level 2+", compCount);
                msg += string.Format("\n# {0} Accounts that have a game/VAC ban", utCount);
                msg += string.Format("\n# {0} Accounts ready to play competitive```", compCount - cdCount - ut2Count);
                await Context.Channel.SendMessageAsync(msg);
            }
            else if (text == "cooldown")
            {
                string msg = string.Format("```md\n{0} STEAM ACCOUNTS\n------------------", steamAccounts.Count);
                int splitCount = 0;
                int cdCount = 0;
                foreach (SteamAccount acct in steamAccounts)
                {
                    if (splitCount >= 30)
                    {
                        msg += "\n```";
                        await Context.Channel.SendMessageAsync(msg);
                        msg = "```md\n";
                        splitCount = 0;
                    }
                    if (acct.CooldownEnd.CompareTo(DateTime.Now) > 0)
                    {
                        cdCount++;
                        msg += string.Format("\n[{0}](level {1})", acct.Username, acct.Level);
                        TimeSpan timeLeft = acct.CooldownEnd.Subtract(DateTime.Now);
                        msg += string.Format("(cooldown ends in{2}{3}{4})", acct.Username, acct.Level, (timeLeft.Days <= 0) ? "" : $" {timeLeft.Days} days, ",
                            (timeLeft.Hours % 24 > 0) ? $" {timeLeft.Hours} hours," : "", (timeLeft.Minutes % 60 > 0) ? $" {timeLeft.Minutes} minutes" : "");
                        splitCount++;
                    }
                }
                msg += string.Format("\n\n# {0} Accounts with Cooldown```", cdCount);
                await Context.Channel.SendMessageAsync(msg);
            }
            else if (text == "level 2" || text == "comp")
            {
                SteamAccount acct = steamAccounts[taCount++];
                while (acct.CooldownEnd.CompareTo(DateTime.Now) > 0) { acct = steamAccounts[taCount++]; if (taCount >= steamAccounts.Count - 1) taCount = 0; }
                while (acct.Level == 1) { acct = steamAccounts[taCount++]; if (taCount >= steamAccounts.Count - 1) break; }
                string msg = string.Format("```ml\n{0} - \"Level {1}\"\nUSERNAME - '{2}'\nPASSWORD - '{3}'\n```", acct.Username, acct.Level, acct.Login, acct.Password);
                await Context.Channel.SendMessageAsync("Sending login info...");
                await Utility.DirectMessage(Context, msg);
            }
            else if (text == "level 1" || text == "new")
            {
                SteamAccount acct = steamAccounts[taCount++];
                while (acct.CooldownEnd.CompareTo(DateTime.Now) > 0) { acct = steamAccounts[taCount++]; if (taCount >= steamAccounts.Count - 1) taCount = 0; }
                while(acct.Level != 1) { acct = steamAccounts[taCount++]; if (taCount >= steamAccounts.Count - 1) break; }
                string msg = string.Format("```ml\n{0} - \"Level {1}\"\nUSERNAME - '{2}'\nPASSWORD - '{3}'\n```", acct.Username, acct.Level, acct.Login, acct.Password);
                await Context.Channel.SendMessageAsync("Sending login info...");
                await Utility.DirectMessage(Context, msg);
            }
            else if (text == "restart")
            {
                taCount = 0;
                await Context.Channel.SendMessageAsync("> Steam account retrieval reset!");
            }
        }

        [Command("UpdateAcct")]
        [Alias("steamlevel", "levelupdate", "talvl")]
        [Summary("Update the CSGO throwaway account level and/or username - Usage: {0}UpdateAcct <login username of account> <csgo level> <new username(optional)>")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task UpdateAcct(string login = "", uint level = 1,[Remainder]string newUsername = "")
        {
            if (login == "") { await Context.Channel.SendMessageAsync("Invalid usage for command!"); return; }
            string path = "D:/Steam/Accounts.json";
            string strFile = File.ReadAllText(path);
            List<SteamAccount> steamAccounts = JsonConvert.DeserializeObject<List<SteamAccount>>(strFile);

            string username = "";
            foreach (SteamAccount account in steamAccounts)
            {
                if (account.Login == login)
                {
                    username = account.Username;
                    if (newUsername != "") account.Username = newUsername;
                    account.Level = level; break;
                }
            }
            File.WriteAllText(path, JsonConvert.SerializeObject(steamAccounts, Formatting.Indented));
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync($"> **{username} updated!**\n> *Level*: {level}" + ((newUsername != "") ? $"\n> Username changed from *{username}* to *{newUsername}*" : ""));
            
        }

        [Command("Cooldown")]
        [Alias("addcooldown", "csban", "steamban")]
        [Summary("Add a cooldown to a CSGO throwaway account - Usage: {0}Cooldown <login username of account> <days> <hours>")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task UpdateAcct(string login = "", int days = 0, int hours = 0)
        {
            if (login == "") { await Context.Channel.SendMessageAsync("Invalid usage for command!"); return; }
            string path = "D:/Steam/Accounts.json";
            string strFile = File.ReadAllText(path);
            List<SteamAccount> steamAccounts = JsonConvert.DeserializeObject<List<SteamAccount>>(strFile);
            string username = "";
            TimeSpan timeLeft = new TimeSpan();
            foreach (SteamAccount account in steamAccounts)
            {
                if (account.Login == login)
                {
                    username = account.Username;
                    account.AddCooldown(days, hours);
                    timeLeft = account.CooldownEnd.Subtract(DateTime.Now); break;
                }
            }
            File.WriteAllText(path, JsonConvert.SerializeObject(steamAccounts, Formatting.Indented));
            string cdMsg = string.Format("ends in{0}{1}{2}", (timeLeft.Days <= 0) ? "" : $" {timeLeft.Days} days ",
                (timeLeft.Hours % 24 > 0) ? $" {timeLeft.Hours}  hours " : "", (timeLeft.Minutes % 60 > 0) ? $" {timeLeft.Minutes} minutes" : "");
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync($"> **{username} updated!**\n> Cooldown {cdMsg}");
        }

        [Command("RemoveAcct")]
        [Alias("taremove", "csremove", "vac")]
        [Summary("Remove an account from the CSGO throwaway database - Usage: {0}RemoveAcct <login username of account>")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task RemoveAcct([Remainder]string login = "")
        {
            if (login == "") { await Context.Channel.SendMessageAsync("Command requires the username login for the account!"); return; }
            string path = "D:/Steam/Accounts.json";
            string strFile = File.ReadAllText(path);
            List<SteamAccount> steamAccounts = JsonConvert.DeserializeObject<List<SteamAccount>>(strFile);
            string username = "";
            SteamAccount toRemove = null;
            foreach (SteamAccount account in steamAccounts)
            {
                if (account.Login == login)
                {
                    username = account.Username;
                    toRemove = account; break;
                }
            }
            steamAccounts.Remove(toRemove);
            File.WriteAllText(path, JsonConvert.SerializeObject(steamAccounts, Formatting.Indented));
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync($"> **{username} successfully removed from database!**");
        }


        [Command("CheckAccts")]
        [Alias("tacheck", "cscheck", "steamcheck")]
        [Summary("Updates all accounts in the database to current username and level - Usage: {0}CheckAccts <args>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CheckAccts([Remainder]string text = "")
        {
            string path = "D:/Steam/Accounts.json"; // Update to dynamic
            string strFile = File.ReadAllText(path);
            List<SteamAccount> steamAccounts = JsonConvert.DeserializeObject<List<SteamAccount>>(strFile);
            await Context.Channel.SendMessageAsync("This may take a while...");
            //tracker.gg api: 43147ded-104d-41f1-9292-379e9ef017a4

            string cracked = "D:/Steam/Cracked.txt";// Update to dynamic
            string[] strCrFile = File.ReadAllLines(cracked);
            int accCount = 0;
            int oldTotal = (text != "username") ? steamAccounts.Count : 0;// If username is passed set to 0
            foreach(string line in strCrFile)
            {
                string[] userpass = line.Split(':');
                string user = userpass[0];
                bool userExist = false;
                foreach(SteamAccount account in steamAccounts)
                {
                    if (account.Login == user) { userExist = true; break; }
                }
                if (userExist) continue;
                string pass = userpass[1];
                SteamAccount steamAccount = new SteamAccount()
                {
                    CooldownEnd = DateTime.Now,
                    CooldownStart = DateTime.Now,
                    Level =  1,
                    Login = user,
                    Password = pass,
                    Username = user
                };
                steamAccounts.Add(steamAccount);
                accCount++;
            }
            int ncCount = 0;// Name Change count variable
            for (int i = oldTotal; i < steamAccounts.Count; i++)
            {
                if (Program.client.ConnectionState == ConnectionState.Disconnected) await Program.client.StartAsync();
                if (text == "username") if (!steamAccounts[i].Username.Contains("Unassigned Name")) continue;// skip whole check if username argument passed
                Steam steam = new Steam();
                steam.Login(steamAccounts[i].Login, steamAccounts[i].Password);
                if (steam.personaname == "" || steam.personaname == null) steam.personaname = "Unassigned Name";
                if (steam.hasBan || steam.hasVac) steam.personaname += "(UNTESTED)";// Remove accounts after first filter or make sure they cant CSGO
                if (steam.personaname != steamAccounts[i].Username)
                {
                    ncCount++;
                    Console.WriteLine($"Changed username {steamAccounts[i].Username} to {steam.personaname}");
                }
                steamAccounts[i].Username = steam.personaname;
                steam.Disconnect();
            }
            File.WriteAllText(path, JsonConvert.SerializeObject(steamAccounts, Formatting.Indented));
            await Context.Channel.SendMessageAsync($"> Successfully added {accCount} cracked accounts to database\n> and updated {ncCount} account's username!");
        }




        /*
         * ------------------------------------------- *
         *   You need to create a function for email   *
         *   generation and or inbox with imap?        *
         *   Either way create a method for attack     *
         *   detection and more function calls!        *
         *   Add a function for hack/csgo team!        *
         *   Add a function for steam authentication   *
         *   for users? Add a call for reporting and   *
         *   commending csgo players (zonerbot style)  *
         * ------------------------------------------- *
         */


        [Command("RequestDJ")]
        [Alias("dj", "musicrole", "musicperms", "djrole")]
        [Summary("Gives requesting user the permission to type in #music - Usage: {0}dj")]
        public async Task RequestDJ(SocketGuildUser user = null)
        {
            SocketRole djRole = null;
            foreach(SocketRole role in Context.Guild.Roles)
            {
                if(role.Name == "DJ") { djRole = role; break; }
            }
            if(djRole == null)
            {
                await Context.Channel.SendMessageAsync($"DJ Role not found!");
                return; 
            }

            if (user == null) user = (SocketGuildUser)Context.User;

            if (user != null)
            {
                if (user.Roles.Count < 1)
                {
                    await Context.Channel.SendMessageAsync($"> **_{user}_** must at least be a **Fresh Byscuit**!");
                    return;
                }
                if (user.Roles.Contains(djRole))
                {
                    await Context.Channel.SendMessageAsync($"> **_{user}_** already has the DJ role!");
                    return;
                }
                else
                {
                    await user.AddRoleAsync(djRole);
                    await Context.Channel.SendMessageAsync($"> **_{user}_** has been given the DJ role!");
                }
            }
        }


        static HttpClientHandler hcHandle = new HttpClientHandler();

        [Command("CheckTwitch")]
        [Alias("live", "twitchlive", "twitch", "stream")]
        [Summary("Checks if the user is live on Twitch - Usage: {0}checktwitch <username>")]
        public async Task CheckTwitch([Remainder]string user = "")
        {
            using (var hc = new HttpClient(hcHandle, false))
            // false here prevents disposing the handler, which should live for the duration of the program and be shared by all requests that use the same handler properties
            {
                hc.DefaultRequestHeaders.Add("Client-ID", Program.config.TWITCH_CLIENT_ID);
                hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "2gbdx6oar67tqtcmt49t3wpcgycthx");
                hc.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/6.0;)");
                hc.Timeout = TimeSpan.FromSeconds(30); // good idea to set to something reasonable

                // https://api.twitch.tv/helix/search/channels?query=
                // https://api.twitch.tv/helix/streams?user_login=
                using (var response = await hc.GetAsync($"https://api.twitch.tv/helix/search/channels?query={user}"))
                {
                    Utility.printConsole(await response.Content.ReadAsStringAsync());
                    response.EnsureSuccessStatusCode(); // throws, if fails, can check response.StatusCode yourself if you prefer
                    string jsonString = await response.Content.ReadAsStringAsync();
                    // TODO: parse json and return true, if the returned array contains the stream

                    Utility.printConsole(jsonString);
                    Twitch.Response r = JsonConvert.DeserializeObject<Twitch.Response>(jsonString);
                    
                }
            }

        }

        #region Utility
        private string[] SplitMessage(string text, char delimiter)
        {
            return text.Split(delimiter);
        }
        /// <summary>
        /// Check to see if a user has a role by name
        /// </summary>
        /// <param name="user">The user that must contain the role</param>
        /// <param name="roleName">The name of the role</param>
        /// <returns>The bool value if the user has the role</returns>
        private bool HasRole(SocketGuildUser user, string roleName)
        {
            SocketRole[] roles = Context.Guild.Roles.ToArray();
            SocketRole reqRole = null;
            foreach (SocketRole role in roles) { if (role.Name == roleName) { reqRole = role; break; } }
            if (user.Roles.Contains(reqRole)) return true;
            return false;
        }

        /// <summary>
        /// Create the embed easier
        /// </summary>
        /// <returns></returns>
        private Embed CreateEmbed()
        {
            return null;
        }
        #endregion
    }
}
