using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules
{
    public class Utility
    {
        static Random rand = new Random();

        /// <summary>
        ///Print to console with the current timestamp
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public static void printConsole(string message)
        {
            Console.WriteLine(DateTime.Now.ToLocalTime() + " | " + message);
        }


        public static async Task DirectMessage(SocketCommandContext Context ,string msg = "")
        {
            // Get or create the Direct channel then send message
            var x = await Context.User.GetOrCreateDMChannelAsync();
            await x.SendMessageAsync(msg);

            // Print to console that we sent the message
            string username = Context.User.Username + "#" + Context.User.Discriminator;
            string text = $"{username} was sent a private message";
            printConsole(text);
        }

        public static int RandomNum(int min, int max)
        {
            // Generate a few random calls first then return it
            rand.Next(min, max); rand.Next(min, max); rand.Next(min, max); rand.Next(min, max);
            return rand.Next(min, max);
        }
    }
}
