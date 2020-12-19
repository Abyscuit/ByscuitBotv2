using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                string text = DateTime.Now + $" | Config not found configured! Please fix in {fullpath} and restart!";
                Console.WriteLine(text);
                Console.ReadLine();
                Environment.Exit(0);
            }
            else
            {
                string contents = File.ReadAllText(fullpath);
                c = JsonConvert.DeserializeObject<Config>(contents);
            }
            return c;
        }
    }
}
