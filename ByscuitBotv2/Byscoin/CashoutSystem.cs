using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Byscoin
{
    public class CashoutSystem
    {
        public static List<CashoutClaim> cashoutClaims = new List<CashoutClaim>();
        public class CashoutClaim
        {
            public ulong DiscordID;
            public uint ClaimID;
            public decimal BYSCAmount;
            public decimal ETHAmount;
            public string Username;
            public string TransactionHash;
            public CashoutState State;
            public string ETHTransactionHash;
            public string ETHAddress;
        }
        public enum CashoutState
        {
            Pending,
            Completed,
            Declined,
            Cancelled
        }
        static string path = "Resources/";
        static string file = "CashoutClaims.json";
        static string fullpath = path + file;

        public static void Save()
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllText(fullpath, JsonConvert.SerializeObject(cashoutClaims, Formatting.Indented));
        }

        public static void Load()
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!File.Exists(fullpath))
            {
                File.WriteAllText(fullpath, JsonConvert.SerializeObject(cashoutClaims, Formatting.Indented));
                string text = DateTime.Now + $" | Generated new file for cashout claims: {fullpath}";
                Console.WriteLine(text);
            }
            else
            {
                string contents = File.ReadAllText(fullpath);
                cashoutClaims = JsonConvert.DeserializeObject<List<CashoutClaim>>(contents);
            }
        }
    }
}
