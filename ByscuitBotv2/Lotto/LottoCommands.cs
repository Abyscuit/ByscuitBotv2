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
using ByscuitBotv2.Data;

namespace ByscuitBotv2.Lotto
{
    public class LottoCommands : ModuleBase<SocketCommandContext>
    {
        // try to figure out how to do internal transfers for gasless tips
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
                SocketGuildUser user = Context.User as SocketGuildUser;
                string username = (string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname) + "#" + user.Discriminator;
                LottoEntry entry = new LottoEntry();
                entry.numbers = new int[] {
                    int.Parse(numArr[0]),
                    int.Parse(numArr[1]),
                    int.Parse(numArr[2]),
                    int.Parse(numArr[3]),
                };
                entry.discordID = Context.User.Id;
                double ETHUSDValue = Nanopool.GetPrices().price_usd;
                decimal BYSCUSDValue = (decimal)ETHUSDValue / 1000000000m;
                LottoSystem.AddEntry(entry);
                int userEntries = LottoSystem.GetUserEntries(user.Id);
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor("Byscoin Lottery Entry", Context.Guild.IconUrl);
                embed.WithColor(36, 122, 191);
                embed.Description = $"`{username} entry has been submitted`";
                embed.WithFields(new EmbedFieldBuilder[]{
                new EmbedFieldBuilder().WithIsInline(true).WithName("Total POT").WithValue($"{LottoSystem.LOTTO_POT} BYSC (${LottoSystem.LOTTO_POT * BYSCUSDValue:N2})"),
                new EmbedFieldBuilder().WithIsInline(true).WithName($"{username} Entries").WithValue($"{userEntries}"),
            });
                embed.WithFooter(new EmbedFooterBuilder() { Text = $"Total Entries: {LottoSystem.LOTTO_ENTRIES.Count}" });
                Context.Channel.SendMessageAsync(embed: embed.Build());
            }));
            bgThread.Start();
            await Task.CompletedTask;
        }

        
    }
}
