using ByscuitBotv2.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Data
{
    public class Accounts
    {
        public class Account
        {
            public ulong DiscordID;
            public TimeSpan TimeSpent;
            public DateTime SessionStart;
            public bool isCounting;

            public Account()
            {
                DiscordID = 0;
                TimeSpent = new TimeSpan();
                SessionStart = DateTime.Now;
                isCounting = false;
            }

            public Account(ulong discordID)
            {
                DiscordID = discordID;
                TimeSpent = new TimeSpan();
                SessionStart = DateTime.Now;
                isCounting = false;
            }
            public int GetHours()
            {
                return TimeSpent.Hours;
            }
            public void StartSession()
            {
                SessionStart = DateTime.Now;
                isCounting = true;
            }
            public void EndSession()
            {
                if (isCounting)
                {
                    TimeSpent = TimeSpent.Add(DateTime.Now.Subtract(SessionStart));
                    isCounting = false;
                }
            }

            public void UpdateTime()
            {
                if (isCounting)
                {
                    TimeSpent = TimeSpent.Add(DateTime.Now.Subtract(SessionStart));
                    SessionStart = DateTime.Now;
                }
            }

            // If return negative then the other account has more time spent
            public int CompareTime(Account account)
            {
                int dif = 0;
                dif = (int)(TimeSpent.TotalSeconds - account.TimeSpent.TotalSeconds);
                return dif;
            }

            public bool isSame(Account account)
            {
                if (DiscordID == account.DiscordID) return true;
                return false;
            }
        }

        public static List<Account> accounts = new List<Account>();

        static string path = "Resources/";
        static string file = "Accounts.json";
        static string fullpath = path + file;

        public static void Save()
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllText(fullpath, JsonConvert.SerializeObject(accounts, Formatting.Indented));
        }

        public static void Load()
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!File.Exists(fullpath))
            {
                File.WriteAllText(fullpath, JsonConvert.SerializeObject(accounts, Formatting.Indented));
                string text = $"Generated new file for accounts: {fullpath}";
                Utility.printConsole(text);
            }
            else
            {
                string contents = File.ReadAllText(fullpath);
                accounts = JsonConvert.DeserializeObject<List<Account>>(contents);
            }

        }

        public static void UpdateUser(ulong discordID, bool counting, bool joined = false)
        {
            bool found = false;
            Account acc = new Account();
            foreach (Account account in accounts)
            {
                if (account.DiscordID == discordID)
                {
                    found = true;
                    acc = account;
                    break;
                }
            }
            if (!found)
            {// Create a new account and add it to the list if not found
                acc = new Account(discordID);
                accounts.Add(acc);
                acc = accounts[accounts.Count - 1];// Just in case reset the reference
            }
            if (counting)
            {
                if (joined) acc.StartSession();
                else acc.UpdateTime();
            }
            else acc.EndSession();
            Save();// Save file
        }

        public static Account GetUser(ulong discordID)
        {
            foreach (Account account in accounts) if (account.DiscordID == discordID) return account;
            Account newAcc = new Account(discordID);
            accounts.Add(newAcc);
            return accounts[accounts.Count - 1];
        }
        public static int GetUserIndex(ulong discordID)
        {
            for(int i = 0;i<accounts.Count;i++) if (accounts[i].DiscordID == discordID) return i;
            return -1;
        }

        // Bruteforce sort
        public static void Sort()
        {
            int total = accounts.Count;
            Account largest = accounts[0];
            List<Account> sortedAccounts = new List<Account>();
            List<Account> arrCopy = new List<Account>();
            arrCopy.AddRange(accounts);
            for(int i =0;i<total;i++) // Iterate through the sorted list
            {
                largest = arrCopy[0];
                foreach (Account account in arrCopy) // For each account 
                {
                    if (account.isSame(largest)) continue;// Skip if largest = current
                    if (sortedAccounts.Contains(account)) continue;// Skip if current is in the list already

                    account.UpdateTime();// Update the time to get the current time
                    if (account.CompareTime(largest) > 0)// If current greater than the largest
                        largest = account;
                }
                sortedAccounts.Add(largest);
                arrCopy.Remove(largest);
            }
            accounts = sortedAccounts;
            Save();
        }

        public static void BinarySort()
        {

        }
    }
}
