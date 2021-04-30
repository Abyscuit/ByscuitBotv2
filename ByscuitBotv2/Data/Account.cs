using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Data
{
    // Credits accounts
    public class Account
    {
        public ulong discordID;
        public string username;
        public string discriminator;
        public double credits;
        public string[] redeemed;
        public double lastMinedAmount;
        public double totalMined;
        public List<Transaction> transactions = new List<Transaction>();
    }

    public class Transaction
    {
        public ulong senderID;
        public ulong receipientID;
        public double amount;
        public string notes;
        public string data;
    }

    public class CreditsSystem
    {
        struct FileTemplate
        {
            public List<Account> accounts;
            public double TotalSupply;
        }
        public static List<Account> accounts = new List<Account>();
        static string path = "Resources/";
        static string file = "CredAccounts.json";
        public static string fullpath = path + file;
        public static double totalcredits = 0;
        public static void LoadAccounts(SocketGuild guild)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!File.Exists(fullpath)) { CreateFile(guild); return; } // Create the file later
            FileTemplate decodedFile = JsonConvert.DeserializeObject<FileTemplate>(File.ReadAllText(fullpath));
            accounts = decodedFile.accounts;
            totalcredits = decodedFile.TotalSupply;
        }


        public static void CreateFile(SocketGuild guild)
        {
            // Write the actual stuff
            foreach(Accounts.Account acc in Accounts.accounts)
            {
                Account account = new Account();
                SocketGuildUser user = guild.GetUser(acc.DiscordID);
                if (user == null) continue;
                double totaldays = user.JoinedAt.Value.Subtract(DateTime.Now).TotalDays * -1;
                account.discordID = user.Id;
                account.discriminator = user.Discriminator;
                account.username = user.Username;
                account.credits = 0;
                account.redeemed = null;
                account.lastMinedAmount = 0;
                account.totalMined = 0;
                account.transactions = new List<Transaction>();
                
                double premiumTime = (user.PremiumSince.HasValue ? user.PremiumSince.Value.Subtract(DateTime.Now).TotalDays * -1 : 0);
                account.credits = totaldays * 2.2 + premiumTime  * 4.5;
                if (user.Hierarchy > 30) account.credits += 69.69 * 30;
                else account.credits += 69.69 * user.Hierarchy;
                account.credits += acc.GetHours() * 42.069;
                totalcredits += account.credits;
                accounts.Add(account);
            }
            foreach (Account acc in accounts)
            {
                SocketGuildUser user = guild.GetUser(acc.discordID);
                if (user == null) continue;
                double totaldays = user.JoinedAt.Value.Subtract(DateTime.Now).TotalDays * -1;

                Console.WriteLine("User: " + (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username));
                Console.WriteLine("Total Days: " + totaldays);
                double premiumTime = (user.PremiumSince.HasValue ? user.PremiumSince.Value.Subtract(DateTime.Now).TotalDays * -1 : 0);
                Console.WriteLine("Hierarchy: " + user.Hierarchy);
                Console.WriteLine("Premium Days: " + premiumTime);
                Console.WriteLine("Credits: " + acc.credits + " | " +  string.Format("{0:p}" , (acc.credits/totalcredits)) +"\n-------------------");
            }
            Console.WriteLine("Total Credits Supply: " + totalcredits);
            SaveFile();
        }

        public static void SaveFile()
        {
            File.WriteAllText(fullpath, JsonConvert.SerializeObject(new FileTemplate() { accounts = accounts, TotalSupply = totalcredits }, Formatting.Indented));
        }

        public static Account GetAccount(SocketGuildUser user)
        {
            if (user == null) return null;

            foreach (Account account in accounts)
                if (user.Id == account.discordID) return account;

            return null;
        }

        public static void MineCoins(List<SocketGuildUser> rewardingAccounts)
        {
            double minedAmount = 0;
            foreach(SocketGuildUser user in rewardingAccounts)
            {
                double minedAmt = 0;
                minedAmt += (user.Hierarchy > 30 ? 30 : user.Hierarchy) * 0.25; // Hierarchy bonus
                double chatAmount = (10 - ((Accounts.GetUserIndex(user.Id) != -1) ? Accounts.GetUserIndex(user.Id) : 9)) * 0.66;
                minedAmt += (chatAmount > 0 && chatAmount <= 12) ? chatAmount : 1; // Chat leaderboard
                minedAmt += user.GuildPermissions.ToList().Count * 0.25; // Guild permissions
                minedAmt += user.Roles.Contains(user.Guild.GetRole(765403412568735765)) ? 20 : 0; // Server booster
                Account account = GetAccount(user);
                if (account == null)
                {
                    account = new Account();
                    account.discordID = user.Id;
                    account.discriminator = user.Discriminator;
                    account.username = user.Username;
                    account.redeemed = null;
                    account.credits = minedAmt;
                    account.lastMinedAmount = minedAmt;
                    account.totalMined = minedAmt;
                    account.transactions = new List<Transaction>();
                    accounts.Add(account);
                }
                else
                {
                    account.credits += minedAmt;
                    account.lastMinedAmount = minedAmt;
                    account.totalMined += minedAmt;
                }
                Console.WriteLine(user.Username + " mined " + minedAmt);
                minedAmount += minedAmt;
            }
            Console.WriteLine("Total Mined: " + minedAmount);
            totalcredits += minedAmount;
            SaveFile();
        }
    }
}
