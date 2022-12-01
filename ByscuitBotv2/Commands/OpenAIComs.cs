using byscuitBot;
using ByscuitBotv2.Data;
using ByscuitBotv2.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Commands
{
    public class OpenAIComs : ModuleBase<SocketCommandContext>
    {
        [Command("AIImage")]
        [Alias("CreateAI", "AIImg", "imgai", "AIArt")]
        [Summary("Create an AI Generated image using Open.AI API (1 credit ($0.04) each use) - Usage: {0}AIImage <prompt>")]
        public async Task AIGenerateImage([Remainder]string prompt)
        {
            SocketGuildUser user = (SocketGuildUser)Context.User;
            Account account = CreditsSystem.GetAccount(user);
            if (account.credits <= 0)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to purchase more credits to use this function!**");
                return;
            }
            if (!(user.Roles.Contains(CommandHandler.PremiumByscuitRole) || user.Roles.Contains(CommandHandler.AIUserRole))
                && !user.GuildPermissions.Administrator)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to be a server booster or have a paid membership to use this function!**");
                return;
            }
            try
            {
                string imageUrl = OpenAI.createImage(prompt, "1", "1024x1024");
                string imgName = "AI-image" + new Random((int)DateTime.Now.Ticks).Next(0, int.MaxValue);
                string tempPath = Directory.GetCurrentDirectory() + $"/AI-Images/";
                string fullImgName = $"{imgName}.png";
                string fullPath = tempPath + fullImgName;
                using (WebClient client = new WebClient())
                {
                    if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                    Utility.printConsole("Fullpath to AI Image: " + fullPath);
                    client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) => {
                        account.credits -= 1;
                        CreditsSystem.SaveFile();
                        string msg = $"> **{Context.User.Mention}'s AI Generated Image** *({account.credits} Credits left)*\n> **Prompt:** __{prompt}__";
                        Context.Channel.SendFileAsync(new FileAttachment(fullPath), msg).Wait();
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                    };
                    client.DownloadFileAsync(new Uri(imageUrl), fullPath);
                }
            }
            catch(Exception e)
            {
                if (e.Message.Equals("The remote server returned an error: (400) Bad Request."))
                    await Context.Channel.SendMessageAsync("> **_Prompt failed!_** **Make sure you dont have __naughty words__ or __public figures__!**");
            }
        }

        [Command("GiveCredits")]
        [Alias("GiveArtCredits", "GiveAICredits", "GiveAICred")]
        [RequireOwner]
        [Summary("Give Credits to a user - Usage: {0}GiveCredits <@user> <amount>")]
        public async Task GiveCredits(int amount, SocketGuildUser user = null)
        {
            if(user == null) user = (SocketGuildUser)Context.User;
            Account account = CreditsSystem.GetAccount(user);
            account.credits += amount;
            CreditsSystem.SaveFile();
            await Context.Channel.SendMessageAsync($"> {user} has {account.credits} AI Credits");
        }

        [Command("ResetCredits")]
        [Alias("ResetAICredit", "ResetAICred")]
        [RequireOwner]
        [Summary("Reset AI Credits for all users - Usage: {0}ResetCredits")]
        public async Task ResetCredits([Remainder]string prompt = "")
        {
            foreach(SocketGuildUser user in Context.Guild.Users)
            {
                if (user.IsBot) continue;
                Account account = CreditsSystem.GetAccount(user);
                if (user.Roles.Contains(CommandHandler.PremiumByscuitRole) ||
                    user.Roles.Contains(CommandHandler.AIUserRole) ||
                    user.GuildPermissions.Administrator)
                {
                    account.credits = 50;
                }
            }
            CreditsSystem.SaveFile();
            await Context.Channel.SendMessageAsync("**All Boosters/Admins have 50 AI credits!**");
        }
    }
}
