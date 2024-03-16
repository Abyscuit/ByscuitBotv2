using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
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

        // Log Levels
        // 0 = Critical, 1 = Error, 2 = Warning, 3 = Info, 4 = Verbose, 5 = Debug
        static Discord.LogSeverity DEBUG_LEVEL = Discord.LogSeverity.Debug; 
        static string[] DEBUG_LVL_STR = { "CRITICAL", "ERROR", "WARNING", "INFO", "VERBOSE", "DEBUG" };

        #region Console Printing Functions
        /// <summary>
        /// Print text to the console with timestamp.
        /// </summary>
        /// <param name="message">The message to print.</param>
        public static void printConsole(string message)
        {
            string msg = DateTime.Now.ToLocalTime() + " | " + message;
            Console.WriteLine(msg);
            log.Add(msg);
        }
        /// <summary>
        /// Print object to the console with timestamp.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        public static void printConsole(object obj)
        {
            string msg = DateTime.Now.ToLocalTime() + " | " + JsonConvert.SerializeObject(obj);
            Console.WriteLine(msg);
            log.Add(msg);
        }

        /// <summary>
        /// Print text to the console as an error with timestamp.
        /// </summary>
        /// <param name="message">The message to print.</param>
        public static void printERROR(string message)
        {
            if ((int)DEBUG_LEVEL >= 1) printConsole($"ERROR | {message}");
        }
        /// <summary>
        /// Print object to the console as an error with timestamp.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        public static void printERROR(object obj)
        {
            if ((int)DEBUG_LEVEL >= 1) printConsole($"ERROR | {JsonConvert.SerializeObject(obj)}");
        }

        /// <summary>
        /// Print text to the console with the debug tag and timestamp.
        /// </summary>
        /// <param name="message">The message to print.</param>
        public static void printDEBUG(string message)
        {
            if ((int)DEBUG_LEVEL >= 5) printConsole($"DEBUG | {message}");
        }
        /// <summary>
        /// Print object to the console with the debug tag and timestamp.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        public static void printDEBUG(object obj)
        {
            if ((int)DEBUG_LEVEL >= 5) printConsole($"DEBUG | {JsonConvert.SerializeObject(obj)}");
        }

        /// <summary>
        /// Print text to the console with the log tag and timestamp.
        /// </summary>
        /// <param name="message">The message to print.</param>
        public static void printLOG(string message)
        {
            if ((int)DEBUG_LEVEL >= 3) printConsole($"INFO | {message}");
        }
        /// <summary>
        /// Print object to the console with the log tag and timestamp.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        public static void printLOG(object obj)
        {
            if ((int)DEBUG_LEVEL >= 3) printConsole($"INFO | {JsonConvert.SerializeObject(obj)}");
        }
        #endregion

        #region Messaging Functions
        public static async Task<IUserMessage> DirectMessage(SocketGuildUser user, string msg = "")
        {
            // Get or create the Direct channel then send message
            var x = await user.CreateDMChannelAsync();

            var sentMsg = await x.SendMessageAsync(msg);

            // Print to console that we sent the message
            string username = user.ToString();
            string text = $"{username} was sent a private message";
            printLOG(text);
            return sentMsg;
        }
        public static async Task<IUserMessage> DirectMessage(SocketGuildUser user,string msg = "", Embed embed = null)
        {
            // Get or create the Direct channel then send message
            var x = await user.CreateDMChannelAsync();
            var sentMsg = await x.SendMessageAsync(msg, false, embed);
            

            // Print to console that we sent the message
            string username = user.ToString();
            string text = $"{username} was sent a private embedded message";
            printLOG(text);
            return sentMsg;
        }
        #endregion

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
            // string logString = log.Count > 0 ? log[0] : "";
            // for (int i = 1; i < log.Count; i++) { logString += log[i]; }
            using (StreamReader sr = new StreamReader(Console.OpenStandardOutput()))
            {
                File.WriteAllText(savePath, sr.ReadToEnd());
            }
        }

        public static void SetDebugLevel(Discord.LogSeverity dbgLvl)
        {
            DEBUG_LEVEL = dbgLvl;
        }

        public static SocketGuildUser[] GetUndefeanedUsersFromChannel(SocketGuildChannel channel)
        {
            List<SocketGuildUser> users = new List<SocketGuildUser>();
            foreach (SocketGuildUser user in channel.Users)
            {
                if (user == null || user.IsBot) continue;
                if(!user.IsSelfDeafened) users.Add(user);
            }
            return users.ToArray();
        }

        // Add separate string by words
    }
}
