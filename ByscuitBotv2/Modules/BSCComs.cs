using ByscuitBotv2.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules
{
    public class BSCComs : ModuleBase<SocketCommandContext>
    {
        static string MAIN_NET = "https://bsc-dataseed1.binance.org:443";
        static string TEST_NET = "https://data-seed-prebsc-1-s1.binance.org:8545";
        static string CURRENT_NET = TEST_NET; // Set the network to work on here
        [Command("BNBRegister")]
        [Alias("bscregister", "bnbenable", "bscenable")]
        [Summary("Link your discord account to a Binance Smart Chain Wallet - Usage: {0}BNBRegister <password (optional)>")]
        public async Task Register([Remainder]string password = "")
        {
            if (password == "") password = "ByscuitBros";
            else await Context.Message.DeleteAsync();
            SocketGuildUser user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            BinanceWallet.WalletAccount account = BinanceWallet.GetAccount(user);
            EmbedBuilder embed = new EmbedBuilder();
            if (account != null)
            {
                await Context.Channel.SendMessageAsync($"> **{username}**_({user.Id})_ already has a BNB wallet linked!");
                return;
            }
            string BNBIcon = "https://s2.coinmarketcap.com/static/img/coins/200x200/1839.png";
            string BNBUrl = "https://coinmarketcap.com/currencies/binance-coin/";
            account = BinanceWallet.CreateAccount(user.Id, password);
            BinanceWallet.AddAccountToList(account);
            embed.WithAuthor("Binance Coin Wallet", BNBIcon, BNBUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            string message = $"`Successfully created and linked a new wallet to {username}`";
            embed.Description = message;
            embed.WithFields(new EmbedFieldBuilder[] { new EmbedFieldBuilder().WithIsInline(false).WithName("Address").WithValue(account.Address)});
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Block Number: " + await BinanceWallet.web3.Eth.Blocks.GetBlockNumber.SendRequestAsync() });
            embed.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }


        [Command("BNBWallet")]
        [Alias("bscwallet", "bnb")]
        [Summary("Show the amount of Binance Coins in your wallet - Usage: {0}BNBWallet")]
        public async Task Wallet(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            BinanceWallet.WalletAccount account = BinanceWallet.GetAccount(user);
            if (account == null) 
            {
                await Context.Channel.SendMessageAsync($"> **{username}**_({user.Id})_ has no Binance Wallet linked!"+
                    "\n> Use the **BNBRegister** command to link a wallet.");
                return;
            }
            // Make it private message only later
            //if (!Context.IsPrivate) await Context.Channel.SendMessageAsync($"> Use this command in a private message only!");
            EmbedBuilder embed = new EmbedBuilder();
            string BNBIcon = "https://s2.coinmarketcap.com/static/img/coins/200x200/1839.png";
            string BNBUrl = "https://coinmarketcap.com/currencies/binance-coin/";
            embed.WithAuthor("Binance Coin Wallet", BNBIcon, BNBUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            embed.WithFields(
                new EmbedFieldBuilder[]{
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Balance").WithValue(string.Format("{0:N8}", await account.GetBalance())),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Transaction Count").WithValue(string.Format("{0}", await BinanceWallet.web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address))),
                    // Display more... Creation date? Transaction Count?
                });
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Block Number: " + await BinanceWallet.web3.Eth.Blocks.GetBlockNumber.SendRequestAsync() });
            embed.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("BNBDeposit")]
        [Alias("bscdeposit", "depositbnb")]
        [Summary("Show your deposit address for Binance Smart Chain wallet - Usage: {0}BNBDeposit")]
        public async Task Deposit()
        {
            //decimal minimumDeposit = 0.01m;
            var user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            BinanceWallet.WalletAccount account = BinanceWallet.GetAccount(user);
            EmbedBuilder embed = new EmbedBuilder();
            if (account == null)
            {
                await Context.Channel.SendMessageAsync($"> **{username}**_({user.Id})_ has no Binance Wallet linked!" +
                    "\n> Use the **BNBRegister** command to link a wallet.");
                return;
            }
            string BNBIcon = "https://s2.coinmarketcap.com/static/img/coins/200x200/1839.png";
            string BNBUrl = "https://coinmarketcap.com/currencies/binance-coin/";
            embed.WithAuthor("Binance Coin Wallet", BNBIcon, BNBUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            embed.WithFields(new EmbedFieldBuilder[]{ new EmbedFieldBuilder().WithIsInline(false).WithName(username).WithValue(account.Address)});
            embed.Description = $"`Deposit only BEP-20 BNB and Tokens.`";
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Block Number: " + await BinanceWallet.web3.Eth.Blocks.GetBlockNumber.SendRequestAsync() });
            embed.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("BNBWithdraw")]
        [Alias("bscwithdraw", "withdrawbnb")]
        [Summary("Withdraw BNB from your Binance Smart Chain wallet - Usage: {0}BNBWithdraw <amount> <address>")]
        public async Task Withdraw(decimal amount = -1, string address = "")
        {
            if (amount == -1 || address == "")
            {
                string msg = "> Withdraw command called incorrectly!" +
                    "\n> Usage: **BNBWithdraw** *<amount>* *<address>*";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            decimal minimumWithdraw = 0.01m;
            var user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            BinanceWallet.WalletAccount account = BinanceWallet.GetAccount(user);
            EmbedBuilder embed = new EmbedBuilder();
            if (account == null)
            {
                await Context.Channel.SendMessageAsync($"> **{username}**_({user.Id})_ has no Binance Wallet linked!" +
                    "\n> Use the **BNBRegister** command to link a wallet.");
                return;
            }

            Nethereum.Util.AddressUtil addressUtil = new Nethereum.Util.AddressUtil();
            if (!addressUtil.IsValidEthereumAddressHexFormat(address))
            {
                string msg = $"> The address {address} is not a valid address!"+
                    $"> Double check you are entering it correctly!";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            string BNBIcon = "https://s2.coinmarketcap.com/static/img/coins/200x200/1839.png";
            string BNBUrl = "https://coinmarketcap.com/currencies/binance-coin/";
            embed.WithAuthor("Binance Coin Withdraw", BNBIcon, BNBUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            bool minimumMet = amount >= minimumWithdraw;
            if (!minimumMet)
            {
                string msg = $"> Minimum amount you can withdraw is {minimumWithdraw} BNB.";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            TransactionReceipt receipt = await account.SendBNB(address, amount);
            if (receipt.Failed()) embed.Description = $"`Withdraw has failed.`";
            else
            {
                embed.Description = $"`Withdraw has been sent.`";
                embed.WithFields(new EmbedFieldBuilder[] {
                    new EmbedFieldBuilder().WithIsInline(false).WithName("Hash").WithValue($"`{receipt.TransactionHash}`"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Amount Sent").WithValue(amount),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Balance Left").WithValue(await account.GetBalance()),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Gas Used").WithValue(Web3.Convert.FromWei(receipt.GasUsed)/ 100000000m),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Index").WithValue(receipt.TransactionIndex),
                });
            }
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Block Number: " + await BinanceWallet.web3.Eth.Blocks.GetBlockNumber.SendRequestAsync() });
            embed.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("BNBTip")]
        [Alias("bsctip", "tipbnb")]
        [Summary("Tip BNB from your Binance Smart Chain wallet to another user - Usage: {0}BNBTip <amount> <@user>")]
        public async Task Tip(decimal amount = -1, SocketGuildUser receipient = null, [Remainder]string text = "")
        {
            decimal minimumTip = 0.0001m;
            var user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            if (amount == -1 || receipient == null)
            {
                string msg = "> Tip command called incorrectly!" +
                    "\n> Usage: **BNBTip** *<amount>* *<@user>*";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            BinanceWallet.WalletAccount account = BinanceWallet.GetAccount(user);
            var receipientAccount = BinanceWallet.GetAccount(receipient);
            string rUsername = (!string.IsNullOrEmpty(receipient.Nickname) ? receipient.Nickname : receipient.Username) + "#" + receipient.Discriminator;
            EmbedBuilder embed = new EmbedBuilder();
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

            string address = receipientAccount.Address;
            string BNBIcon = "https://s2.coinmarketcap.com/static/img/coins/200x200/1839.png";
            string BNBUrl = "https://coinmarketcap.com/currencies/binance-coin/";
            embed.WithAuthor("Binance Coin Tip", BNBIcon, BNBUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            bool minimumMet = amount >= minimumTip;
            if (!minimumMet)
            {
                string msg = $"> Minimum amount you can tip is {minimumTip} BNB.";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            TransactionReceipt receipt = await account.SendBNB(address, amount);
            if (receipt.Failed()) embed.Description = $"`Tip failed to send! Try again in a bit.`";
            else
            {
                embed.Description = $"`Tip has been sent to {rUsername}`";
                embed.WithFields(new EmbedFieldBuilder[] {
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Amount Sent").WithValue(amount),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Balance Left").WithValue(await account.GetBalance()),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Gas Used").WithValue(Web3.Convert.FromWei(receipt.GasUsed) / 100000000m)
                });
            }
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Block Number: " + await BinanceWallet.web3.Eth.Blocks.GetBlockNumber.SendRequestAsync() });
            embed.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
