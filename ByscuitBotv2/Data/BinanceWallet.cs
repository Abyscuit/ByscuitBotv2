using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.HdWallet;
using System.IO;
using Newtonsoft.Json;
using Discord.WebSocket;
using System.Numerics;
using NBitcoin;
using Nethereum.RPC.Eth.DTOs;
using System.Net;
using Nethereum.Contracts;

namespace ByscuitBotv2.Data
{
    public class BinanceWallet
    {
        static string MAIN_NET = "https://bsc-dataseed1.binance.org:443";
        static string TEST_NET = "https://data-seed-prebsc-1-s1.binance.org:8545";
        static string CURRENT_NET = TEST_NET;
        public static Web3 web3 = new Web3(CURRENT_NET);
        static Random random = new Random();

        public class WalletAccount
        {
            public ulong DiscordID;
            // Store whole wallet object maybe?
            //public Wallet Wallet;
            public string Words; // Wont let me store words as a string array
            public string Password;
            public string Seed;
            public int Index;
            public string PrivateKey;
            public string Address;

            public WalletAccount(string words, string pass, ulong discordID)
            {
                Index = 0; // Hardcoded index for now
                Password = pass; // ------------ Encode the password!!!!!! -------------
                Words = words;
                Wallet wallet = new Wallet(Words, Password);
                var Account = wallet.GetAccount(Index);
                
                PrivateKey = Account.PrivateKey;
                Seed = wallet.Seed;
                Address = Account.Address;
                DiscordID = discordID;
            }

            public Wallet GetWallet()
            {
                return new Wallet(Words, Password);
            }

            public Nethereum.Web3.Accounts.Account GetAccount()
            {
                return GetWallet().GetAccount(Index);
            }

            public async Task<TransactionReceipt> SendBNB(string to, decimal amount)
            {
                Web3 sweb3 = new Web3(GetAccount(), CURRENT_NET);
                TransactionReceipt receipt = await sweb3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(to, amount);
                return receipt;
            }

            public async Task<decimal> GetBalance()
            {
                Console.WriteLine($"Address: {Address}");
                BigInteger bal = await web3.Eth.GetBalance.SendRequestAsync(Address);
                Console.WriteLine($"Balance (GWEI): {bal}");
                decimal result = Web3.Convert.FromWei(bal);
                Console.WriteLine($"Balance (BNB): {result}");
                return result;
            }
        }
        static string[] wordsList = null;
        static Wordlist words = Wordlist.LoadWordList(Language.English).Result;
        public static string GenerateWordList()
        {
            random.Next();random.Next();random.Next();
            string Words = "";
            int count = 0;
            wordsList = words.GetWords().ToArray<string>();
            for (int j = 0; j<wordsList.Length;j++)
            {
                string word = wordsList[random.Next(0,wordsList.Length - 1)];
                if (Words.Contains(word)) continue; // skip if the seed has the word already
                Words += word;
                count++;
                if (count == 12) break; // Break out the loop once we have 12 words
                Words += " "; // Add a space until the second to last word
            }
            return Words;
        }

        public static List<WalletAccount> accounts = new List<WalletAccount>();

        static string path = "Resources/";
        static string file = "BinanceWallets.json";
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
                string text = DateTime.Now + $" | Generated new file for binance wallets: {fullpath}";
                Console.WriteLine(text);
            }
            else
            {
                string contents = File.ReadAllText(fullpath);
                accounts = JsonConvert.DeserializeObject<List<WalletAccount>>(contents);
            }

        }
        public static WalletAccount GetAccount(SocketGuildUser user)
        {
            if (user == null) return null;

            foreach (WalletAccount account in accounts)
                if (user.Id == account.DiscordID) return account;

            return null;
        }

        public static WalletAccount CreateAccount(ulong discordID, string password = "ByscuitBros")
        {
            WalletAccount account = new WalletAccount(GenerateWordList(), password, discordID);
            return account;
        }

        public static void AddAccountToList(WalletAccount account)
        {
            if(!accounts.Contains(account))
            {
                bool isDuplicate = false;
                foreach(WalletAccount acc in accounts)
                {
                    if(acc.DiscordID == account.DiscordID)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate) { accounts.Add(account); Save(); }
            }
        }


        public class BinanceAPI
        {
            static string url = "https://api.binance.com";
            static string avgPrice = "/api/v3/avgPrice";

            public static string GetETHPairing()
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + avgPrice+"?symbol=BNBETH");
                request.ContentType = "application/json; charset=utf-8";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                string data = "";
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    data = reader.ReadToEnd();
                }

                BinanceResponse r = JsonConvert.DeserializeObject<BinanceResponse>(data);
                return r.price;
            }

            struct BinanceResponse
            {
                public int mins;
                public string price;
            }
        }
    }
}
