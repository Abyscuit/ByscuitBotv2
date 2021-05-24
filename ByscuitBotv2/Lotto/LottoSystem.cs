using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Lotto
{
    public class LottoSystem
    {
        public class LottoFile
        {
            public List<LottoEntry> LOTTO_ENTRIES;
            public decimal LOTTO_POT;
        }
        public static List<LottoEntry> LOTTO_ENTRIES = new List<LottoEntry>();
        public static int LOTTO_PRICE = 100;
        public static decimal INITIAL_LOTTO_POT = 100000; // Initial Pot winnings ($0.30)
        public static decimal LOTTO_POT = INITIAL_LOTTO_POT;

        static string path = "Resources/";
        static string file = "LottoEntries.json";
        static string fullpath = path + file;

        public static void Save()
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            LottoFile lFile = new LottoFile() { LOTTO_POT = LOTTO_POT, LOTTO_ENTRIES = LOTTO_ENTRIES };
            File.WriteAllText(fullpath, JsonConvert.SerializeObject(lFile, Formatting.Indented));
        }

        public static void Load()
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!File.Exists(fullpath))
            {
                LottoFile lFile = new LottoFile() { LOTTO_POT = LOTTO_POT, LOTTO_ENTRIES = LOTTO_ENTRIES };
                File.WriteAllText(fullpath, JsonConvert.SerializeObject(lFile, Formatting.Indented));
                string text = DateTime.Now + $" | Generated new file for Lotto Entries: {fullpath}";
                Console.WriteLine(text);
            }
            else
            {
                string contents = File.ReadAllText(fullpath);
                LottoFile lFile = JsonConvert.DeserializeObject<LottoFile>(contents);
                LOTTO_ENTRIES = lFile.LOTTO_ENTRIES;
                LOTTO_POT = lFile.LOTTO_POT;
            }
        }

        public static void AddEntry(LottoEntry lottoEntry)
        {
            LOTTO_ENTRIES.Add(lottoEntry);
            LOTTO_POT += LOTTO_PRICE;
            Save();
        }

        public static int GetUserEntries(ulong discordID)
        {
            int count = 0;
            for(int i = 0; i < LOTTO_ENTRIES.Count; i++) if (LOTTO_ENTRIES[i].discordID == discordID) count++;
            return count;
        }

    }
}
