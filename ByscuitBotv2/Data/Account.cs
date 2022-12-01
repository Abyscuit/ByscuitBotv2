using ByscuitBotv2.Modules;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ByscuitBotv2.Data
{
    // OpenAI accounts
    public class Account
    {
        public ulong discordID;
        public string username;
        public string discriminator;
        public int credits;
        public string[] redeemed;
        public List<Transaction> transactions = new List<Transaction>();
    }

    public class Transaction
    {
        public ulong senderID;
        public ulong receipientID;
        public string depositAddress;
        public double amount;
        public string notes;
        public string data;
    }

    public class CreditsSystem
    {
        public static List<Account> accounts = new List<Account>();
        static string path = "Resources/";
        static string file = "CredAccounts.json";
        public static string fullpath = path + file;
        public static void LoadAccounts(SocketGuild guild)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!File.Exists(fullpath)) { CreateFile(guild); return; } // Create the file later
            accounts = JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(fullpath));
        }


        public static void CreateFile(SocketGuild guild)
        {
            // Write the actual stuff
            guild.GetUsersAsync().ForEachAsync(m => {
                foreach(Discord.IGuildUser user in m)
                {
                    if (user.IsBot) continue; // Dont create an account for the bots
                    Account account = new Account();
                    account.discordID = user.Id;
                    account.discriminator = user.Discriminator;
                    account.username = user.Username;
                    account.credits = 0;
                    account.redeemed = null;
                    account.transactions = new List<Transaction>();
                    accounts.Add(account);
                    double totaldays = user.JoinedAt.Value.Subtract(DateTime.Now).TotalDays * -1;

                    Utility.printDEBUG("User: " + (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username));
                    Utility.printDEBUG("Total Days: " + totaldays);
                    double premiumTime = (user.PremiumSince.HasValue ? user.PremiumSince.Value.Subtract(DateTime.Now).TotalDays * -1 : 0);
                    Utility.printDEBUG("Premium Days: " + premiumTime);
                    Utility.printDEBUG("Credits: " + account.credits + "\n-------------------");
                }
            }).Wait();
            SaveFile();
        }
        public static Account AddUser(SocketGuildUser user)
        {
            if (user.IsBot) return null; // Dont create an account for the bots
            if (GetAccount(user) != null) return null; // Dont create duplicates
            Account account = new Account();
            account.discordID = user.Id;
            account.discriminator = user.Discriminator;
            account.username = user.Username;
            account.credits = 0;
            account.redeemed = null;
            account.transactions = new List<Transaction>();
            accounts.Add(account);
            double totaldays = user.JoinedAt.Value.Subtract(DateTime.Now).TotalDays * -1;

            Utility.printDEBUG("User: " + (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username));
            Utility.printDEBUG("Total Days: " + totaldays);
            double premiumTime = (user.PremiumSince.HasValue ? user.PremiumSince.Value.Subtract(DateTime.Now).TotalDays * -1 : 0);
            Utility.printDEBUG("Premium Days: " + premiumTime);
            Utility.printDEBUG("Credits: " + account.credits + "\n-------------------");
            SaveFile();
            return account;
        }
        public static void SaveFile()
        {
            File.WriteAllText(fullpath, JsonConvert.SerializeObject(accounts, Formatting.Indented));
        }

        public static Account GetAccount(SocketGuildUser user)
        {
            if (user == null) return null;

            foreach (Account account in accounts)
                if (user.Id == account.discordID) return account;

            return null;
        }

    }
}
