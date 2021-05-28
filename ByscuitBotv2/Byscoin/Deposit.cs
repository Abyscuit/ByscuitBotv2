using byscuitBot;
using ByscuitBotv2.Data;
using ByscuitBotv2.Modules;
using Discord.WebSocket;
using Nethereum.Web3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Byscoin
{
    public class Deposit
    {
        public enum DepositState
        {
            Pending,
            Completed,
            Cancelled
        }
        public class DepositClaim
        {
            public string depositAddress;
            public DepositState state;
            public ulong discordID;
            public DateTime timeStamp;
            public string transactionHash;
        }

        public static List<DepositClaim> depositClaims = new List<DepositClaim>();
        public static bool NEW_DEPOSITS = false;

        static string path = "Resources/";
        static string file = "DepositClaims.json";
        public static string fullpath = path + file;
        public static void Load()
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!File.Exists(fullpath)) { SaveFile(); return; } // Create the file later
            depositClaims = JsonConvert.DeserializeObject<List<DepositClaim>>(File.ReadAllText(fullpath));
        }

        public static void SaveFile()
        {
            File.WriteAllText(fullpath, JsonConvert.SerializeObject(depositClaims, Formatting.Indented));
        }

        public static void CreateDepositClaim(string DepositAddress, ulong DiscordID)
        {
            DepositClaim claim = new DepositClaim()
            {
                state = DepositState.Pending,
                depositAddress = DepositAddress,
                discordID = DiscordID,
                timeStamp = DateTime.Now,
                transactionHash = null
            };
            depositClaims.Add(claim);
            SaveFile();
            NEW_DEPOSITS = true;
            DEPOSIT_TIMER = DateTime.Now;
        }
        public static DateTime DEPOSIT_TIMER = DateTime.Now; // Should set it to the time last deposit claim was made
        public static void CheckDepositClaims()
        {
            string BSCSCAN_API_ENDPOINT = "https://api.bscscan.com/api?module=account&action=tokentx&address=" + ByscComs.POOL_ADDRESS + "&startblock=0&endblock=92500000&sort=asc&apikey=" + Program.config.BSCSCAN_API_KEY;
            string BSCSCANTEST_API_ENDPOINT = "https://api-testnet.bscscan.com/api?module=account&action=tokentx&address=" + ByscComs.POOL_ADDRESS + "&startblock=0&endblock=92500000&sort=asc&apikey=" + Program.config.BSCSCAN_API_KEY;
            List<DepositClaim> toRemove = new List<DepositClaim>();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BSCSCANTEST_API_ENDPOINT);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Timeout = 5000;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.101 Safari/537.36";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            string data = "";
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                data = reader.ReadToEnd();
            }
            BEP20_Response r = JsonConvert.DeserializeObject<BEP20_Response>(data);

            foreach (DepositClaim claim in depositClaims)
            {
                // If there is a non-pending claim remove it after 30 minutes
                if (claim.state != DepositState.Pending)
                {
                    if (DateTime.Now.Subtract(claim.timeStamp).TotalMinutes > 30) { depositClaims.Remove(claim); continue; }
                }
                else
                {
                    // If the claim is older than 30 mins throw it out
                    if (DateTime.Now.Subtract(claim.timeStamp).TotalMinutes > 30) {
                        if(claim.transactionHash == null) claim.state = DepositState.Cancelled;
                        depositClaims.Remove(claim); continue;
                    }
                    for (int i = r.result.Length - 1; i > 0; i--)
                    {
                        BEP20_Transaction transaction = r.result[i];
                        if (transaction.contractAddress.ToUpper() != ByscComs.CONTRACT_ADDRESS.ToUpper()) continue;
                        if (transaction.from.ToUpper() == claim.depositAddress.ToUpper())
                        {
                            // Check to see if a transaction has been claimed already
                            bool usedHash = false;
                            foreach(DepositClaim c in depositClaims)
                            {
                                if (c == claim) continue; // Skip if same claim
                                if(c.transactionHash != null)
                                {
                                    if(c.transactionHash.ToUpper() == transaction.hash.ToUpper())
                                    {
                                        usedHash = true;
                                        break; 
                                    }
                                }
                            }
                            if (usedHash) continue; // Skip if we already paid
                            // Check if the transaction is older than 30 minutes
                            if (DateTime.Now.Subtract(UnixTimeStampToDateTime(double.Parse(transaction.timeStamp))).TotalMinutes > 30) continue;
                            SocketGuildUser user = CommandHandler.Byscuits.GetUser(claim.discordID);
                            Account acc = CreditsSystem.GetAccount(user);
                            claim.transactionHash = transaction.hash;
                            acc.credits += (double)Web3.Convert.FromWei(BigInteger.Parse(transaction.value));
                            claim.state = DepositState.Completed;
                            CreditsSystem.SaveFile();
                            user.GetOrCreateDMChannelAsync().Result.SendMessageAsync($"> Confirmed transaction of {(double)Web3.Convert.FromWei(BigInteger.Parse(transaction.value))} BYSC");
                            break;
                        }
                    }
                }
            }
            SaveFile();
            if(DateTime.Now.Subtract(DEPOSIT_TIMER).TotalMinutes > 30) { NEW_DEPOSITS = false; }
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public class BEP20_Transaction
        {
            public string blockNumber,
                timeStamp,
                hash,
                nonce,
                blockHash,
                from,
                to,
                contractAddress,
                value,
                tokenName,
                tokenSymbol,
                tokenDecimal,
                transactionIndex,
                gas, gasPrice,
                gasUsed,
                cumulitaveGasUsed,
                input, confirmations;
        }
        public class BEP20_Response
        {
            public string status, message;
            public BEP20_Transaction[] result;
        }
    }
}
