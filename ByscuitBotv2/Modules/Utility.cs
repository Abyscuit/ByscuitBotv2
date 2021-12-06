using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules
{
    public class Utility
    {
        static Random rand = new Random();
        public static List<string> log = new List<string>();
        static uint DEBUG_LEVEL = 3; // 0 = OFF, 1 = ERROR, 2 = LOG, 3 = DEBUG || More settings in the future
        //static string[] DEBUG_LVL_STR = { "OFF", "ERROR", "LOG", "DEBUG" };

        /// <summary>
        ///Print to console with the current timestamp
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public static void printConsole(string message)
        {
            string msg = DateTime.Now.ToLocalTime() + " | " + message;
            Console.WriteLine(msg);
            log.Add(msg);
        }
        public static void printERROR(string message)
        {
            if (DEBUG_LEVEL >= 1) printConsole($"ERROR | {message}");
        }

        public static void printDEBUG(string message)
        {
            if (DEBUG_LEVEL >= 3) printConsole($"DEBUG | {message}");
        }
        public static void printLOG(string message)
        {
            if (DEBUG_LEVEL >= 2) printConsole($"LOG | {message}");
        }

        public static async Task DirectMessage(SocketGuildUser user, string msg = "")
        {
            // Get or create the Direct channel then send message
            var x = await user.GetOrCreateDMChannelAsync();
            await x.SendMessageAsync(msg);

            // Print to console that we sent the message
            string username = user.Username + "#" + user.Discriminator;
            string text = $"{username} was sent a private message";
            printLOG(text);
        }
        public static async Task DirectMessage(SocketGuildUser user,string msg = "", Embed embed = null)
        {
            // Get or create the Direct channel then send message
            var x = await user.GetOrCreateDMChannelAsync();
            await x.SendMessageAsync(msg, false, embed);

            // Print to console that we sent the message
            string username = user.Username + "#" + user.Discriminator;
            string text = $"{username} was sent a private embedded message";
            printLOG(text);
        }

        public static int RandomNum(int min, int max)
        {
            // Generate a few random calls first then return it
            rand.Next(min, max); rand.Next(min, max); rand.Next(min, max); rand.Next(min, max);
            return rand.Next(min, max);
        }

        public static void SaveLog()
        {
            string saveFolder = "Resources";
            string saveFile = $"{DateTime.Now.ToString("dd")}";
            string savePath = $"{saveFolder}/{saveFile}";
            if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
            string logString = log.Count > 0 ? log[0] : "";
            for (int i = 1; i < log.Count; i++) { logString += log[i]; }
            File.WriteAllText(savePath, logString);
        }

        public static void SetDebugLevel(uint dbgLvl)
        {
            DEBUG_LEVEL = dbgLvl;
        }
    }
}
