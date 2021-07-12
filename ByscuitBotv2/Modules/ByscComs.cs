using byscuitBot;
using ByscuitBotv2.Byscoin;
using ByscuitBotv2.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ByscuitBotv2.Byscoin.CashoutSystem;
using static ByscuitBotv2.Data.SmartContract;

namespace ByscuitBotv2.Modules
{
    public class ByscComs : ModuleBase<SocketCommandContext>
    {
        public static string CONTRACT_ADDRESS = "0x8926E6a13B628947b440d0a820C8496fC728a14A";
        public static string POOL_ADDRESS = "0xE4a555DAF0c71aBeF7b2d725EEFAe41deaD4D8dD";
        static string MAIN_NET = "https://bsc-dataseed1.binance.org:443";
        static string TEST_NET = "https://data-seed-prebsc-1-s1.binance.org:8545";
        public static string CURRENT_NET = MAIN_NET; // Set the network to work on here

        public decimal byscBNBValue = 871596;

        #region internal token interaction
        // RECODE ALL SO IT WILL BE USED FOR INTERNAL TRANSFERS
        [Command("Wallet")]
        [Alias("byscoin", "coins", "bal", "balance")]
        [Summary("Show the amount of Byscoin in your wallet - Usage: {0}Wallet")]
        public async Task Wallet(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            Account account = CreditsSystem.GetAccount(user);
            EmbedBuilder embed = new EmbedBuilder();
            if (account == null) account = CreditsSystem.AddUser(user);

            string strBNBValue = BinanceWallet.BinanceAPI.GetUSDPairing();
            double BNBValue = double.Parse(strBNBValue);
            decimal BYSCUSDValue = (decimal)BNBValue/ byscBNBValue;
            embed.WithAuthor("Byscoin Wallet", Context.Guild.IconUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            embed.WithFields(new EmbedFieldBuilder[]{
                new EmbedFieldBuilder().WithIsInline(true).WithName("Holding").WithValue(string.Format("{0:p}", account.credits/CreditsSystem.totalcredits)),
                new EmbedFieldBuilder().WithIsInline(true).WithName(username).WithValue($"{account.credits} (${account.credits * (double)BYSCUSDValue:N2})"),
            });
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Total Supply: " + CreditsSystem.totalcredits});
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

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Byscoin Stats", Context.Guild.IconUrl);
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
            }
            if(names == "") { names = "No users"; credits = "None"; }
            embed.WithColor(36, 122, 191);
            embed.WithFields(new EmbedFieldBuilder[]{ new EmbedFieldBuilder().WithIsInline(true).WithName("Users").WithValue(names),
                new EmbedFieldBuilder().WithIsInline(true).WithName("Total Coins").WithValue(credits),});
            embed.WithFooter(new EmbedFooterBuilder() { Text = $"Total Supply: {CreditsSystem.totalcredits}" });
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Tip")]
        [Alias("byscointip", "byscoinsend", "bysctip")]
        [Summary("Tip a user in Byscoin - Usage: {0}Tip <amount> <@user>")]
        public async Task ByscTip(string amount = "-1", SocketGuildUser receipient = null, [Remainder] string text = "")
        {
            // approve the transaction with pool address as spender from tipping account with amount as allowance
            // send the transaction with the pool address account
            decimal minimumTip = 0.05m;
            var user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            if (amount == "-1" || receipient == null)
            {
                string msg = "> Tip command called incorrectly!" +
                    "\n> Usage: **Tip** *<amount>* *<@user>*";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            if (receipient.IsBot)
            {
                await Context.Channel.SendMessageAsync("> Cannot tip a bot!");
                return;
            }
            Account account = CreditsSystem.GetAccount(user);
            var receipientAccount = CreditsSystem.GetAccount(receipient);
            string rUsername = (!string.IsNullOrEmpty(receipient.Nickname) ? receipient.Nickname : receipient.Username) + "#" + receipient.Discriminator;
            // Create user accounts if they do not exist
            if (account == null) account = CreditsSystem.AddUser(user);
            if (receipientAccount == null) receipientAccount = CreditsSystem.AddUser(receipient);

            string strBNBValue = BinanceWallet.BinanceAPI.GetUSDPairing();
            double BNBValue = double.Parse(strBNBValue);
            decimal BYSCUSDValue = (decimal)BNBValue / byscBNBValue;

            decimal byscAmount = 0;
            bool dollarValue = false;
            if (amount.Contains("$")) { amount = amount.Replace("$", ""); dollarValue = true; }
            if (amount.ToUpper() == "ALL") { amount = "" + account.credits; }
            if(!decimal.TryParse(amount, out byscAmount))
            {
                await Context.Channel.SendMessageAsync("> Amount to send was not in the correct format!" +
                    "\n> Amount can be specified in USD by \"$X.XX\" or BYSC amount by omitting '$'");
            }
            if (dollarValue) byscAmount /= BYSCUSDValue;
            if(account.credits < (double)byscAmount)
            {
                await Context.Channel.SendMessageAsync($"> Insufficient balance {user.Mention}");
                return;
            }
            bool minimumMet = byscAmount >= minimumTip;
            if (!minimumMet)
            {
                string msg = $"> Minimum amount you can tip is {minimumTip} BYSC.";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            account.credits -= (double)byscAmount;
            receipientAccount.credits += (double)byscAmount;
            string tipMsg = $"> {username} sent {byscAmount:N} BYSC (${byscAmount * BYSCUSDValue:N2}) to {rUsername}!";
            CreditsSystem.SaveFile();
            await Context.Channel.SendMessageAsync(tipMsg);
        }


        [Command("Deposit")]
        [Alias("byscdeposit", "depositbysc")]
        [Summary("Create a deposit claim and show deposit address for Byscoin wallet (MUST PUT ADDRESS YOU'RE DEPOSITTING FROM)- Usage: {0}Deposit <address>")]
        public async Task Deposit([Remainder] string address = "")
        {
            Nethereum.Util.AddressUtil addressUtil = new Nethereum.Util.AddressUtil();
            if (!addressUtil.IsValidEthereumAddressHexFormat(address) || !addressUtil.IsValidAddressLength(address)
                || !addressUtil.IsNotAnEmptyAddress(address))
            {
                string msg = $"> The address {address} is not a valid address!" +
                    $"\n> Double check you are entering it correctly!";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            //decimal minimumDeposit = 0.01m;
            var user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            Account account = CreditsSystem.GetAccount(user);
            EmbedBuilder embed = new EmbedBuilder();
            if (account == null) account = CreditsSystem.AddUser(user);
            
            embed.WithAuthor("Byscoin Deposit", Context.Guild.IconUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            embed.WithFields(new EmbedFieldBuilder[] { new EmbedFieldBuilder().WithIsInline(false).WithName("Deposit Address").WithValue(POOL_ADDRESS)
            });
            embed.Description = $"```Deposit only BEP20 Byscoin to this address!" +
                $"\nDeposits will be confirmed after 1 confirmation." +
                $"\nIf your deposit takes longer than 30 minutes from now to confirm it won't be counted!" +
                $"\n*****CALL THIS COMMAND FOR EVERY DEPOSIT!*****```";
            embed.WithFooter(new EmbedFooterBuilder() { Text = "Block Number: " + await BinanceWallet.web3.Eth.Blocks.GetBlockNumber.SendRequestAsync() });
            embed.WithCurrentTimestamp();
            Byscoin.Deposit.CreateDepositClaim(address, user.Id);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("Withdraw")]
        [Alias("byscwithdraw", "withdrawbysc")]
        [Summary("Withdraw BYSC from your wallet to a Binance Smart Chain Wallet (BEP-20) - Usage: {0}Withdraw <amount> <address>")]
        public async Task Withdraw(decimal amount = -1, string address = "")
        {
            await Context.Channel.SendMessageAsync("> This command is a work in progress.");
            return;
            if (amount == -1 || address == "")
            {
                string msg = "> Withdraw command called incorrectly!" +
                    "\n> Usage: **Withdraw** *<amount>* *<address>*";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            decimal minimumWithdraw = 0.01m;
            var user = Context.User as SocketGuildUser;
            string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
            Account account = CreditsSystem.GetAccount(user);
            EmbedBuilder embed = new EmbedBuilder();
            if (account == null) account = CreditsSystem.AddUser(user);
            if(account.credits < (double)amount)
            {
                return;
            }

            Nethereum.Util.AddressUtil addressUtil = new Nethereum.Util.AddressUtil();
            if (!addressUtil.IsValidEthereumAddressHexFormat(address))
            {
                string msg = $"> The address {address} is not a valid address!" +
                    $"\n> Double check you are entering it correctly!";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            embed.WithAuthor("Byscoin Coin Withdraw", Context.Guild.IconUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            bool minimumMet = amount >= minimumWithdraw;
            if (!minimumMet)
            {
                string msg = $"> Minimum amount you can withdraw is {minimumWithdraw} BNB.";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            /*TransactionReceipt receipt = await account.SendBNB(address, amount);
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
            */
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
            TimeSpan timeleft = Nanopool.GetTimeUntilPayout(Nanopool.ADDRESS).Subtract(DateTimeOffset.Now);
            double daysleft = timeleft.TotalDays;
            double hoursleft = timeleft.TotalHours;
            double minutesleft = timeleft.TotalMinutes;
            result += "\n**_";
            string strPayMsg = "About {0} to reach payout";
            string strPayTime = "";
            if (daysleft > 30) strPayTime += $"{Math.Round(daysleft / 30.0f)} month(s)";
            else if (daysleft >= 1) strPayTime += $"{Math.Round(daysleft)} day(s)";
            else
            {
                if (hoursleft >= 1) strPayTime += $"{Math.Round(hoursleft)} hour(s) ";
                if (minutesleft > 0) strPayTime += $"{Math.Round(minutesleft % 60)} minute(s)";
            }
            strPayMsg = string.Format(strPayMsg, strPayTime);
            result += strPayMsg;
            result += "_**";
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
        // All Discord Byscoin should be testnet to save money
        // Cashout to real BNB/ETH on mainnet


        public Thread cashoutThread;
        // Created a list for threads so multiple people can cashout
        // at the same time. Need to check for resources used in the running
        // threads as this could lead to file or memory corruption
        /* Might just remove this as there will be a Pancake swap Pool
        public static List<Thread> CASHOUT_THREADS = new List<Thread>(); // Use with caution
        [Command("Cashout")]
        [Alias("byscoincashout", "bysccashout")]
        [Summary("Cashout Byscoin for Ethereum - Usage: {0}Cashout <amount> <address>")]
        public async Task Cashout(decimal amount = -1, string address = "", [Remainder] string text = "")
        {
            //await Context.Channel.SendMessageAsync("> This command is still under development"); return;
            if (cashoutThread != null)
            {
                if (cashoutThread.ThreadState == ThreadState.Running)
                {
                    await Context.Channel.SendMessageAsync("> Wait for the previous cashout to finish");
                    return;
                }
            }
            Nethereum.Util.AddressUtil addressUtil = new Nethereum.Util.AddressUtil(); 
            if(!addressUtil.IsValidAddressLength(address) || !addressUtil.IsValidEthereumAddressHexFormat(address) ||
                addressUtil.IsAnEmptyAddress(address))
            {
                await Context.Channel.SendMessageAsync("> Ethereum Address is not valid!");
                return;
            }
            cashoutThread = new Thread(new ThreadStart(() =>
            {
                // 100000 for debug tests
                decimal minimum = 100000; // 1 million Byscoins ($3)

                var user = Context.User as SocketGuildUser;
                string username = (!string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username) + "#" + user.Discriminator;
                if (amount == -1)
                {
                    string msg = "> Cashout command called incorrectly!" +
                        "\n> Usage: **Cashout** *<amount>* *<address>*";
                    Context.Channel.SendMessageAsync(msg);
                    cashoutThread.Abort();
                    return;
                }
                BinanceWallet.WalletAccount account = BinanceWallet.GetAccount(user);
                if (account == null)
                {
                    Context.Channel.SendMessageAsync($"> **{username}**_({user.Id})_ has no Binance Wallet linked!" +
                        "\n> Use the **BNBRegister** command to link a wallet.");
                    cashoutThread.Abort();
                    return;
                }

                Web3 web3 = new Web3(account.GetAccount(), CURRENT_NET);

                var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
                string ownerAddress = "0xA10106F786610D0CF05796b5F13aF7724A1faC34"; // receive address for byscoin pool
                var transfer = new TransferFunction()
                {
                    To = ownerAddress,
                    TokenAmount = Web3.Convert.ToWei(amount),
                };
                TransactionReceipt transactionReceipt = null;
                try
                {
                    transactionReceipt = transferHandler.SendRequestAndWaitForReceiptAsync(CONTRACT_ADDRESS, transfer).Result;
                }
                catch(Exception ex)
                {
                    Context.Channel.SendMessageAsync($"> {ex.InnerException.Message}");
                    return;
                }


                var balanceOfFunctionMessage = new BalanceOfFunction()
                {
                    Owner = account.Address,
                };

                var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
                var balance = balanceHandler.QueryAsync<BigInteger>(CONTRACT_ADDRESS, balanceOfFunctionMessage).Result;

                double ETHUSDValue = Nanopool.GetPrices().price_usd;
                decimal BYSCUSDValue = (decimal)ETHUSDValue / 1000000000m;
                decimal balVal = Web3.Convert.FromWei(balance);
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor("Byscoin Cashout", Context.Guild.IconUrl);
                embed.WithThumbnailUrl(user.GetAvatarUrl());
                embed.WithColor(36, 122, 191);
                bool minimumMet = amount >= minimum;
                if (!minimumMet)
                {
                    string msg = $"> Minimum amount you can cashout is {minimum} BYSC.";
                    Context.Channel.SendMessageAsync(msg);
                    return;
                }
                decimal amountEth = amount / 1000000000; // amount divided by 1 bil to get ether value
                uint claimID = (uint)CashoutSystem.cashoutClaims.Count;
                CashoutClaim claim = new CashoutClaim()
                {
                    ClaimID = claimID,
                    BYSCAmount = amount,
                    ETHAmount = amountEth,
                    DiscordID = user.Id,
                    TransactionHash = transactionReceipt.TransactionHash,
                    Username = username,
                    State = CashoutState.Pending,
                    ETHTransactionHash = "",
                    ETHAddress = address
                };
                cashoutClaims.Add(claim);
                Save();
                    embed.Description = $"`Cashout claim is now pending manual review`";
                    embed.WithFields(new EmbedFieldBuilder[] {
                    new EmbedFieldBuilder().WithIsInline(false).WithName("Transaction Hash").WithValue($"{transactionReceipt.TransactionHash}"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Claim ID").WithValue($"{claimID}"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Amount Cashed Out").WithValue($"{amount:N0} (${amount * (decimal)BYSCUSDValue:N2})"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Amount in ETH").WithValue($"{amountEth:N8} (${amountEth * (decimal)ETHUSDValue:N2})"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Balance Left").WithValue($"{balVal:N0} (${balVal * (decimal)BYSCUSDValue:N2})"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Gas Used").WithValue((decimal)Web3.Convert.FromWei(transactionReceipt.GasUsed.Value, Nethereum.Util.UnitConversion.EthUnit.Gwei)+ " BNB")
                });
                embed.WithFooter(new EmbedFooterBuilder() { Text = "Block Number: " + transactionReceipt.BlockNumber });
                embed.WithCurrentTimestamp();
                Context.Channel.SendMessageAsync("", false, embed.Build());
            }));
            cashoutThread.Start(); // Start work on different thread (risky!!!!) (CHECK IF THREAD IS ACTIVE!!!)
            await Context.Channel.SendMessageAsync("> Cashout claim sent waiting for transaction...");
        }

        [Command("FinishCashout")]
        [Alias("completecashout", "completeclaim")]
        [Summary("Complete cashout claim - Usage: {0}FinishCashout <claimID> <status> <ETHTransactionHash>")]
        [RequireOwner()]
        public async Task FinishCashout(uint claimID, string status = "CANCELLED", string ETHTransactionHash = "")
        {
            await Context.Message.DeleteAsync();
            status = status.ToUpper();
            if (status != "REJECTED" || status != "CANCELLED") status = "COMPLETED";
            CashoutClaim Claim = null;
            foreach (CashoutClaim claim in cashoutClaims)
            {// Adds option to go back to claim ID if not removed
                if (claim.ClaimID == claimID)
                {
                    claim.State = CashoutState.Completed;
                    claim.ETHTransactionHash = ETHTransactionHash;
                    Claim = claim;
                    break;
                }
            }
            CashoutSystem.Save(); // Save cashout claims
            SocketGuildUser user = Context.Guild.GetUser(Claim.DiscordID);

            double ETHUSDValue = Nanopool.GetPrices().price_usd;
            decimal BYSCUSDValue = (decimal)ETHUSDValue / 1000000000m;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Byscoin Cashout", Context.Guild.IconUrl);
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithColor(36, 122, 191);
            embed.Description = $"`Your cashout claim was {status}`";
            if (status.ToUpper() != "REJECTED" || status.ToUpper() != "CANCELLED")
            {
                embed.WithFields(new EmbedFieldBuilder[] {
                    new EmbedFieldBuilder().WithIsInline(false).WithName("ETH Transaction Hash").WithValue($"{Claim.ETHTransactionHash}"),
                    new EmbedFieldBuilder().WithIsInline(false).WithName("ETH Address").WithValue($"{Claim.ETHAddress}"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Claim ID").WithValue($"{claimID}"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Amount Cashed Out").WithValue($"{Claim.BYSCAmount:N0} (${Claim.BYSCAmount * (decimal)BYSCUSDValue:N2})"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Amount in ETH").WithValue($"{Claim.ETHAmount:N8} (${Claim.ETHAmount * (decimal)ETHUSDValue:N2})")
                });
            }
            else
            {
                embed.WithFields(new EmbedFieldBuilder[] {
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Claim ID").WithValue($"{claimID}"),
                    new EmbedFieldBuilder().WithIsInline(true).WithName("Bycoin Amount").WithValue($"{Claim.BYSCAmount:N0} (${Claim.BYSCAmount * (decimal)BYSCUSDValue:N2})")
                });

            }
            embed.WithCurrentTimestamp();
            await Utility.DirectMessage(user, embed: embed.Build());
        }
        */
        #endregion
    }
}
