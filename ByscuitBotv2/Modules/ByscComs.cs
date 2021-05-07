using byscuitBot;
using ByscuitBotv2.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ByscuitBotv2.Data.SmartContract;

namespace ByscuitBotv2.Modules
{
    public class ByscComs : ModuleBase<SocketCommandContext>
    {
        #region fake token interaction
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
        #endregion

        #region Nanopool
        [Command("Nanopool")]
        [Alias("Nanostats", "mining", "poolstats")]
        [Summary("Show the current nanopool stats for the miners - Usage: {0}Nanopool")]
        public async Task GetNanopoolInfo([Remainder] string text = "")
        {
            Nanopool nanopool = new Nanopool();
            string result = nanopool.BalanceCheck();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Nanopool Stats", Context.Guild.IconUrl);
            embed.WithColor(36, 122, 191);
            embed.Description = result;
            embed.WithFooter(new EmbedFooterBuilder() { Text = $"ETH/USD Value: {nanopool.ethUSDValue} | BNB/ETH Value: {BinanceWallet.BinanceAPI.GetETHPairing()}" });
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("NanopoolPayout")]
        [Alias("Nanopayout", "payout", "nanopay")]
        [Summary("Show the current nanopool stats for the miners - Usage: {0}NanopoolPayout")]
        [RequireOwner()]
        public async Task NanopoolPayout([Remainder] string text = "")
        {
            await Context.Message.DeleteAsync();
            ulong minerRoleID = 832412957396303892; // Role for miners
            SocketRole minerRole = Context.Guild.GetRole(minerRoleID);
            Nanopool nanopool = new Nanopool();
            string result = nanopool.DoPayout();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor($"Official Nanopool Payout {DateTime.Now.ToLocalTime().ToString("M/dd/yyyy")}", Context.Guild.IconUrl);
            embed.WithColor(36, 122, 191);
            embed.Description = result;
            embed.WithFooter(new EmbedFooterBuilder() { Text = $"ETH/USD Value: {nanopool.ethUSDValue} | BNB/ETH Value: {BinanceWallet.BinanceAPI.GetETHPairing()}" });
            await Context.Channel.SendMessageAsync(minerRole.Mention, false, embed.Build());
        }
        #endregion

        #region Byscoin
        public static string CONTRACT_ADDRESS = "0x256639f740364144851cc206063221b2e122337c";
        static string MAIN_NET = "https://bsc-dataseed1.binance.org:443";
        static string TEST_NET = "https://data-seed-prebsc-1-s1.binance.org:8545";
        static string CURRENT_NET = TEST_NET; // Set the network to work on here
        [Command("Byscoin")]
        [Alias("byscbal", "byscwallet")]
        [Summary("Show the amount of Byscoin in your Binance Smart Chain wallet - Usage: {0}Byscoin")]
        public async Task GetBalance([Remainder] string text = "")
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            BinanceWallet.WalletAccount account = BinanceWallet.GetAccount(user);
            if (account == null)
            {
                await Context.Channel.SendMessageAsync($"> **{username}**_({user.Id})_ does not have a Binance Wallet linked!" +
                    "\n> Use the **BNBRegister** command to link a wallet.");
                return;
            }

            Web3 web3 = new Web3(CURRENT_NET);
            var balanceOfFunctionMessage = new BalanceOfFunction()
            {
                Owner = account.Address,
            };

            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balance = await balanceHandler.QueryAsync<BigInteger>(CONTRACT_ADDRESS, balanceOfFunctionMessage);

            var totalSupplyFunctionMessage = new TotalSupplyFunction();

            var totalSupplyHandler = web3.Eth.GetContractQueryHandler<TotalSupplyFunction>();
            var totalSupply = await totalSupplyHandler.QueryAsync<BigInteger>(CONTRACT_ADDRESS, totalSupplyFunctionMessage);

            decimal totalSupplyVal = Web3.Convert.FromWei(totalSupply);

            double ETHUSDValue = Nanopool.GetPrices().price_usd;
            decimal BYSCUSDValue = (decimal)ETHUSDValue / 1000000000m;
            decimal balVal = Web3.Convert.FromWei(balance);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Byscoin Wallet", Context.Guild.IconUrl);
            embed.WithColor(36, 122, 191);
            embed.WithFields(new EmbedFieldBuilder[]{
                new EmbedFieldBuilder().WithIsInline(false).WithName(username).WithValue(account.Address),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Byscoin Balance").WithValue($"{balVal:N8} (${balVal * BYSCUSDValue:N2})"),
            });
            embed.WithFooter(new EmbedFooterBuilder() { Text = $"Total Supply: {totalSupplyVal} (${totalSupplyVal * BYSCUSDValue:N2})" });
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }


        [Command("ByscTip")]
        [Alias("byscointip", "byscoinsend")]
        [Summary("Show the amount of Byscoin in your Binance Smart Chain wallet - Usage: {0}ByscTip")]
        public async Task ByscTip(decimal amount = -1, SocketGuildUser receipient = null, [Remainder] string text = "")
        {
            decimal minimumTip = 0.05m;
            var user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            if (amount == -1 || receipient == null)
            {
                string msg = "> Tip command called incorrectly!" +
                    "\n> Usage: **BYSCTip** *<amount>* *<@user>*";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            BinanceWallet.WalletAccount account = BinanceWallet.GetAccount(user);
            var receipientAccount = BinanceWallet.GetAccount(receipient);
            string rUsername = (!string.IsNullOrEmpty(receipient.Nickname) ? receipient.Nickname : receipient.Username) + "#" + receipient.Discriminator;
            if (account == null)
            {
                await Context.Channel.SendMessageAsync($"> **{username}**_({user.Id})_ has no Binance Wallet linked!" +
                    "\n> Use the **BNBRegister** command to link a wallet.");
                return;
            }
            if (receipientAccount == null)
            {
                await Context.Channel.SendMessageAsync($"> **{rUsername}**_({receipient.Id})_ has no Binance Wallet linked!" +
                    $"\n> {receipient.Mention} use the **BNBRegister** command to link a wallet.");
                return;
            }

            Web3 web3 = new Web3(account.GetAccount(), CURRENT_NET);

            var receiverAddress = receipientAccount.Address;
            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction()
            {
                To = receiverAddress,
                TokenAmount = Web3.Convert.ToWei(amount)
            };
            var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(CONTRACT_ADDRESS, transfer);


            var balanceOfFunctionMessage = new BalanceOfFunction()
            {
                Owner = account.Address,
            };

            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balance = await balanceHandler.QueryAsync<BigInteger>(CONTRACT_ADDRESS, balanceOfFunctionMessage);

            double ETHUSDValue = Nanopool.GetPrices().price_usd;
            decimal BYSCUSDValue = (decimal)ETHUSDValue / 1000000000m;
            decimal balVal = Web3.Convert.FromWei(balance);
            EmbedBuilder embed = new EmbedBuilder();
            string address = receipientAccount.Address;
            embed.WithAuthor("Byscoin Tip", Context.Guild.IconUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            bool minimumMet = amount >= minimumTip;
            if (!minimumMet)
            {
                string msg = $"> Minimum amount you can tip is {minimumTip} BYSC.";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            //if (transactionReceipt.) embed.Description = $"`Tip failed to send! Try again in a bit.`";
            //else
            {
                embed.Description = $"`Tip has been sent to {rUsername}`";
                embed.WithFields(new EmbedFieldBuilder[] {
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Amount Sent").WithValue($"{amount:N8} (${amount * (decimal)BYSCUSDValue:N2})"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Balance Left").WithValue($"{balVal:N8} (${balVal * (decimal)BYSCUSDValue:N2})"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Gas Used").WithValue((decimal)(transactionReceipt.GasUsed.Value) / 100000000m + " BNB")
                });
            }
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Block Number: " + transactionReceipt.BlockNumber });
            embed.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
        #endregion
    }
}
