using byscuitBot;
using ByscuitBotv2.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules
{
    public class ByscComs : ModuleBase<SocketCommandContext>
    {

        [Command("Wallet")]
        [Alias("byscuitcoins", "coins")]
        [Summary("Show the amount of Byscuit Coins in your wallet - Usage: {0}Wallet")]
        public async Task Wallet(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            Account account = CreditsSystem.GetAccount(user);
            EmbedBuilder embed = new EmbedBuilder();
            if (account == null)
            {
                await Context.Channel.SendMessageAsync($"> **{username}**_({user.Id})_ has no Byscuit Coins!");
                return;
            }
            embed.WithAuthor("Byscuit Coin Wallet", Context.Guild.IconUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            embed.WithFields(new EmbedFieldBuilder[]{ new EmbedFieldBuilder().WithIsInline(true).WithName("Holding").WithValue(string.Format("{0:p}", account.credits/CreditsSystem.totalcredits)),
                new EmbedFieldBuilder().WithIsInline(true).WithName(username).WithValue(account.credits),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Total Mined").WithValue(account.totalMined)});
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Total Supply: " + CreditsSystem.totalcredits});
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Miners")]
        [Alias("byscminers", "showminers")]
        [Summary("Show the last miners and the total amount they mined - Usage: {0}Wallet")]
        public async Task Miners([Remainder] string text = "")
        {
            // Check for who is boosting then display their data
            if(CommandHandler.miners.Count == 0)
            {
                await Context.Channel.SendMessageAsync($"> There are currently no miners on record!");
                return;
            }
            string names = "";
            string credits = "";
            string lastmined = "";
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Byscuit Coin Miners", Context.Guild.IconUrl);
            foreach (SocketGuildUser user in CommandHandler.miners)
            {
                string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
                names += username + "\n";
                Account account = CreditsSystem.GetAccount(user);
                if (account == null) continue;
                credits += account.totalMined + "\n";
                lastmined += account.lastMinedAmount + "\n";
            }
            embed.WithColor(36, 122, 191);
            embed.WithFields(new EmbedFieldBuilder[]{ new EmbedFieldBuilder().WithIsInline(true).WithName("Users").WithValue(names),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Total Mined").WithValue(credits),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Last Mined Amount").WithValue(lastmined)});
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Total Supply: " + CreditsSystem.totalcredits });
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }


        [Command("ByscStats")]
        [Alias("ByscuitStats", "coinstats")]
        [Summary("Show the amount of total coins, holders and other data - Usage: {0}ByscStats")]
        public async Task ByscStats([Remainder] string text = "")
        {
            if (CreditsSystem.accounts == null)
            {
                await Context.Channel.SendMessageAsync($"> There are currently no holders on record!");
                return;
            }
            string names = "";
            string credits = "";
            string totalMined = "";
            double totalCredits = 0, totalCoinsMined = 0;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Byscuit Coin Stats", Context.Guild.IconUrl);
            foreach (Account acc in CreditsSystem.accounts)
            {
                SocketGuildUser user = Context.Guild.GetUser(acc.discordID);
                if (user == null) continue;
                if (acc.credits == 0) continue;
                string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
                names += username + "\n";
                Account account = CreditsSystem.GetAccount(user);
                if (account == null) continue;
                credits += account.credits + "\n";
                totalMined += account.totalMined + "\n";
                totalCredits += account.credits;
                totalCoinsMined += account.totalMined;
            }
            CreditsSystem.totalcredits = totalCredits;
            CreditsSystem.SaveFile();
            embed.WithColor(36, 122, 191);
            embed.WithFields(new EmbedFieldBuilder[]{ new EmbedFieldBuilder().WithIsInline(true).WithName("Users").WithValue(names),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Total Coins").WithValue(credits),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Total Mined Amount").WithValue(totalMined)});
            embed.WithFooter(new EmbedFooterBuilder() { Text = $"Total Supply: {CreditsSystem.totalcredits} | Total Mined: {totalCoinsMined}" });
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
