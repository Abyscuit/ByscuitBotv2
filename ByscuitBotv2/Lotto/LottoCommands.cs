using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Net;
using Discord;
using Discord.WebSocket;
using System.Threading;

namespace ByscuitBotv2.Lotto
{
    public class LottoCommands : ModuleBase<SocketCommandContext>
    {
        public static List<LottoEntry> LOTTO_ENTRIES = new List<LottoEntry>();
        public static int LOTTO_PRICE = 100;
        public static decimal LOTTO_POT = 1000000; // Initial Pot winnings
        Thread bgThread; // Thread for background tasking
        [Command("Entry")]
        [Alias("lottoentry", "lotto")]
        [Summary("Enter the Byscoin Lotto choosing 4 numbers for 100 BYSC - Usage: {0}lotto <num> <num> <num> <num>")]
        public async Task Entry([Remainder]string numbers = "")
        {
            await Context.Message.DeleteAsync();
            if (bgThread != null)
            {
                if (bgThread.ThreadState == ThreadState.Running) Console.WriteLine("Background Thread is still running!");
            }
            string[] numArr = numbers.Split(' ');
            if (numbers == "" || numArr.Length != 4)
            {
                await Context.Channel.SendMessageAsync("You must choose 4 numbers! - Usage: {0}lotto <num> <num> <num> <num>");
                return;
            }
            bgThread = new Thread(new ThreadStart(() =>
            {
                LottoEntry entry = new LottoEntry();
                entry.numbers = new int[] {
                    int.Parse(numArr[0]),
                    int.Parse(numArr[1]),
                    int.Parse(numArr[2]),
                    int.Parse(numArr[3]),
                };
                entry.discordID = Context.User.Id;
                Thread.Sleep(2000);
                Context.Channel.SendMessageAsync("Completed");
            }));
            bgThread.Start();
            await Task.CompletedTask;
        }

        
    }
}
