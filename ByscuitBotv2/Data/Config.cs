using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ByscuitBotv2.Modules;
using Newtonsoft.Json;

namespace ByscuitBotv2.Data
{
    public class Config
    {
        public string DISCORD_API_KEY = "";
        public string STEAM_API_KEY = "";
        public string ETH_SCAN_KEY = "";
        public string CMC_API_KEY = "";
        public string GOOGLE_API_KEY = "";
        public string TWITCH_CLIENT_ID = "";
        public string TWITCH_SECRET = "";
        public string BSCSCAN_API_KEY = "";
        public string OPENAI_API_KEY = "";
        public string BFL_API_KEY = "";
        public float NANOPOOL_PAYOUT = 0.4f; // Default level 0.4f

        // Log Levels
        // 0 = Critical, 1 = Error, 2 = Warning, 3 = Info, 4 = Verbose, 5 = Debug
        public Discord.LogSeverity DEBUG_LEVEL = Discord.LogSeverity.Critical; // Default level 0 

        public static Config LoadConfig()
        {
            Config c = new Config();
            string path = "Resources/";
            string file = "Config.ini";
            string fullpath = path + file;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!File.Exists(fullpath))
            {
                File.WriteAllText(fullpath, JsonConvert.SerializeObject(c, Formatting.Indented));
                string text = $"Config not found configured! Please fix in {fullpath} and restart!";
                Utility.printConsole(text);
                Console.ReadLine();
                Environment.Exit(0);
            }

            string contents = File.ReadAllText(fullpath);
            c = JsonConvert.DeserializeObject<Config>(contents);
            c.CheckDebugLevel();
            return c;
        }

        public void CheckDebugLevel()
        {
            if (DEBUG_LEVEL > Discord.LogSeverity.Debug)
            {
                DEBUG_LEVEL = 0;
                SaveConfig();
            }
        }

        public void SaveConfig()
        {
            string path = "Resources/";
            string file = "Config.ini";
            string fullpath = path + file;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (File.Exists(fullpath))
            {
                File.WriteAllText(fullpath, JsonConvert.SerializeObject(this, Formatting.Indented));
                Utility.printConsole("Saved Config successfully!");
            }

        }

        public override string ToString()
        {
            return $"DISCORD_API_KEY: {DISCORD_API_KEY}" +
            $"STEAM_API_KEY: {STEAM_API_KEY}" +
            $"ETH_SCAN_KEY: {ETH_SCAN_KEY}" +
            $"CMC_API_KEY: {CMC_API_KEY}" +
            $"GOOGLE_API_KEY: {GOOGLE_API_KEY}" +
            $"TWITCH_CLIENT_ID: {TWITCH_CLIENT_ID}" +
            $"TWITCH_SECRET: {TWITCH_SECRET}" +
            $"BSCSCAN_API_KEY: {BSCSCAN_API_KEY}" +
            $"OPENAI_API_KEY: {OPENAI_API_KEY}" +
            $"BFL_API_KEY: {BFL_API_KEY}" +
            $"DEBUG_LEVEL: {DEBUG_LEVEL}";
        }
    }
}
