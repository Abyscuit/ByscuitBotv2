using byscuitBot;
using ByscuitBotv2.Data;
using ByscuitBotv2.Modules;
using ByscuitBotv2.Modules.OpenAI;
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
using System.Threading;
using System.Threading.Tasks;

namespace ByscuitBotv2.Commands
{
    public class OpenAIComs : ModuleBase<SocketCommandContext>
    {
        [Command("AIImage")]
        [Alias("CreateAI", "AIImg", "imgai", "AIArt", "aicreate")]
        [Summary("Create an AI Generated image using AI (1 credit ($0.04) each use) - Usage: {0}AIArt <prompt>")]
        public async Task AIGenerateImage([Remainder] string prompt)
        {
            SocketGuildUser user = (SocketGuildUser)Context.User;
            Account account = CreditsSystem.GetAccount(user);
            string number = prompt.Split(' ')[0];
            int numberOfImages = 1;
            /*
            if (int.TryParse(number, out numberOfImages))
            {
                if (numberOfImages > 5) numberOfImages = 1;
                else prompt = prompt.Substring(number.Length);
            }
            */
            if (account.credits < numberOfImages)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to purchase more credits to use this function!**");
                return;
            }/*
            if (!(user.Roles.Contains(CommandHandler.PremiumByscuitRole) || user.Roles.Contains(CommandHandler.AIUserRole))
                && !user.GuildPermissions.Administrator)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to be a server booster or have a paid membership to use this function!**");
                return;
            }*/
            ThreadStart threadStart = new ThreadStart(async () => {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        // TODO: Loop for amount of images
                        // save the image downloaded count and paths in an array
                        // send all images at the end
                        string imageUrl = await Images.createImage(prompt, "1", "1024x1024");
                        string imgName = "AI-image" + new Random((int)DateTime.Now.Ticks).Next(0, int.MaxValue);
                        string tempPath = Directory.GetCurrentDirectory() + $"/AI-Images/";
                        string fullImgName = $"{imgName}.png";
                        string fullPath = tempPath + fullImgName;
                        if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                        Utility.printConsole("Fullpath to AI Image: " + fullPath);
                        client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                        {
                            account.credits -= 1;
                            CreditsSystem.SaveFile();
                            if (account.credits <= 0)
                            {
                                if (user.Roles.Contains(CommandHandler.AIUserRole))
                                    user.RemoveRoleAsync(CommandHandler.AIUserRole).Wait();
                            }
                            string msg = $"> **{Context.User.Mention}'s AI Generated Image** *({account.credits} Credits left)*\n> **Prompt:** __{prompt}__";
                            Context.Channel.SendFileAsync(new FileAttachment(fullPath), msg).Wait();
                            if (File.Exists(fullPath)) File.Delete(fullPath);
                        };
                        client.DownloadFileAsync(new Uri(imageUrl), fullPath);
                    }
                }
                catch (Exception e)
                {
                    Utility.printERROR(e.Message);
                    await Context.Channel.SendMessageAsync(user.Mention +
                        $"\n> **_Prompt failed!_**\n> **{e.Message}**");
                }
            });

            Thread t = new Thread(threadStart);
            t.Start();
        }

        [Command("Dalle3")]
        [Alias("CreateAI3", "AIImg3", "imgai3", "AIArt3", "aicreate3")]
        [Summary("Create an AI Generated image using Dalle-3 (3 credit ($0.12) each use) - Usage: {0}Dalle3 <prompt>")]
        public async Task Dalle3GenerateImage([Remainder] string prompt)
        {
            SocketGuildUser user = (SocketGuildUser)Context.User;
            Account account = CreditsSystem.GetAccount(user);
            bool isHDPrompt = prompt.ToLower().Contains(" HD");
            string number = prompt.Split(' ')[0];
            int numberOfImages = 1;
            int price = isHDPrompt ? 5 : 3;
            /*
            if (int.TryParse(number, out numberOfImages))
            {
                if (numberOfImages > 5) numberOfImages = 1;
                else prompt = prompt.Substring(number.Length);
            }
            */
            if (account.credits < numberOfImages * price)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to purchase more credits to use this function!**");
                return;
            }/*
            if (!(user.Roles.Contains(CommandHandler.PremiumByscuitRole) || user.Roles.Contains(CommandHandler.AIUserRole))
                && !user.GuildPermissions.Administrator)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to be a server booster or have a paid membership to use this function!**");
                return;
            }*/
            ThreadStart threadStart = new ThreadStart(async () => {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        // TODO: Loop for amount of images
                        // save the image downloaded count and paths in an array
                        // send all images at the end
                        string imageUrl = await Images.createImage(prompt, "1", "1024x1024", model: "dall-e-3", quality: (isHDPrompt ? "hd" : "standard"));
                        string imgName = "AI-image" + new Random((int)DateTime.Now.Ticks).Next(0, int.MaxValue);
                        string tempPath = Directory.GetCurrentDirectory() + $"/AI-Images/";
                        string fullImgName = $"{imgName}.png";
                        string fullPath = tempPath + fullImgName;
                        if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                        Utility.printConsole("Fullpath to AI Image: " + fullPath);
                        client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                        {
                            account.credits -= price;
                            CreditsSystem.SaveFile();
                            if (account.credits <= 0)
                            {
                                if (user.Roles.Contains(CommandHandler.AIUserRole))
                                    user.RemoveRoleAsync(CommandHandler.AIUserRole).Wait();
                            }
                            string msg = $"> **{Context.User.Mention}'s AI Generated Image** *({account.credits} Credits left)*\n> **Prompt:** __{prompt}__";
                            Context.Channel.SendFileAsync(new FileAttachment(fullPath), msg).Wait();
                            if (File.Exists(fullPath)) File.Delete(fullPath);
                        };
                        client.DownloadFileAsync(new Uri(imageUrl), fullPath);
                    }
                }
                catch (Exception e)
                {
                    Utility.printERROR(e.Message);
                    await Context.Channel.SendMessageAsync(user.Mention +
                        $"\n> **_Prompt failed!_**\n> **{e.Message}**");
                }
            });

            Thread t = new Thread(threadStart);
            t.Start();
        }

        [Command("AIEdit")]
        [Alias("EditAI", "AIEditImage")]
        [Summary("Edit an image using AI <Must Attach a Square Image w/Transparency> (1 credit ($0.04) each use) - Usage: {0}AIEdit <prompt>")]
        public async Task AIEdit([Remainder]string prompt)
        {
            SocketGuildUser user = (SocketGuildUser)Context.User;
            Account account = CreditsSystem.GetAccount(user);
            if (account.credits <= 0)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to purchase more credits to use this function!**");
                return;
            }/*
            if (!(user.Roles.Contains(CommandHandler.PremiumByscuitRole) || user.Roles.Contains(CommandHandler.AIUserRole))
                && !user.GuildPermissions.Administrator)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to be a server booster or have a paid membership to use this function!**");
                return;
            }*/
            if (Context.Message.Attachments.Count < 1)
            {
                string msg = "> **You need to provide an image to edit!**";
                msg += "\n> The image must be square and contain a transparent mask.";
                await Context.Channel.SendMessageAsync(msg);
                return;
            }
            ThreadStart threadStart = new ThreadStart(async () =>
            {
                try
                {
                    List<string> imgUrls = new List<string>();
                    using (IEnumerator<Attachment> imgEnum = Context.Message.Attachments.GetEnumerator())
                    {
                        while (imgEnum.MoveNext())
                        {
                            imgUrls.Add(imgEnum.Current.Url);
                        }
                    }
                    Console.WriteLine(imgUrls);
                    string imageUrl = await Images.editImage(prompt, "1", "1024x1024", imgUrls[0], imgUrls.Count > 1 ? imgUrls[1] : null);
                    string imgName = "AI-image" + new Random((int)DateTime.Now.Ticks).Next(0, int.MaxValue);
                    string tempPath = Directory.GetCurrentDirectory() + $"/AI-Images/";
                    string fullImgName = $"{imgName}.png";
                    string fullPath = tempPath + fullImgName;
                    using (WebClient client = new WebClient())
                    {
                        if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                        Utility.printConsole("Fullpath to AI Image: " + fullPath);
                        client.DownloadFileCompleted += async (object sender, AsyncCompletedEventArgs e) =>
                        {
                            account.credits -= 1;
                            CreditsSystem.SaveFile();
                            if (account.credits <= 0)
                            {
                                if (user.Roles.Contains(CommandHandler.AIUserRole))
                                    await user.RemoveRoleAsync(CommandHandler.AIUserRole);
                            }
                            string msg = $"> **{Context.User.Mention}'s AI Altered Image** *({account.credits} Credits left)*\n> **Prompt:** __{prompt}__";
                            await Context.Channel.SendFileAsync(new FileAttachment(fullPath), msg);
                            if (File.Exists(fullPath)) File.Delete(fullPath);
                        };
                        client.DownloadFileAsync(new Uri(imageUrl), fullPath);
                    }
                }
                catch (Exception e)
                {
                    Utility.printERROR(e.Message);
                    await Context.Channel.SendMessageAsync(user.Mention +
                        $"\n> **_Prompt failed!_**\n> **{e.Message}**");
                }
            });
            
            Thread t = new Thread(threadStart);
            t.Start();
        }

        [Command("GiveCredits")]
        [Alias("GiveArtCredits", "GiveAICredits", "GiveAICred")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Give Credits to a user - Usage: {0}GiveCredits <amount> <@user>")]
        public async Task GiveCredits(int amount, SocketGuildUser user = null)
        {
            if(user == null) user = (SocketGuildUser)Context.User;
            Account account = CreditsSystem.GetAccount(user);
            account.credits += amount;
            if (account.credits > 0)
            {
                if (!user.Roles.Contains(CommandHandler.AIUserRole))
                    await user.AddRoleAsync(CommandHandler.AIUserRole);
            }
            else if (account.credits <= 0)
            {
                if (user.Roles.Contains(CommandHandler.AIUserRole))
                    await user.RemoveRoleAsync(CommandHandler.AIUserRole);
            }
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
                else account.credits = 5;

                if (!user.Roles.Contains(CommandHandler.AIUserRole))
                    await user.AddRoleAsync(CommandHandler.AIUserRole);
            }
            CreditsSystem.SaveFile();
            await Context.Channel.SendMessageAsync("**All Boosters/Admins have 50 AI credits!**");
        }

        [Command("AIchat")]
        [Alias("AItext", "AItxt", "gpt")]
        [Summary("Create an AI Generated text completion (price based on prompt) - Usage: {0}GPT <prompt>")]
        public async Task AIchat([Remainder]string prompt)
        {
            SocketGuildUser user = (SocketGuildUser)Context.User;
            Account account = CreditsSystem.GetAccount(user);
            if (account.credits <= 0)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to purchase more credits to use this function!**");
                return;
            }
            /*
            if (!(user.Roles.Contains(CommandHandler.PremiumByscuitRole) || user.Roles.Contains(CommandHandler.AIUserRole))
                && !user.GuildPermissions.Administrator)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to be a server booster or have a paid membership to use this function!**");
                return;
            }
            */
            ThreadStart threadStart = new ThreadStart(async () => {
                try
                {
                    GPT_3.Response response = await GPT_3.CreateCompletion(prompt);
                    double creditsUsed = (double)response.usage.total_tokens / 1000.0000 * 1.5;
                    creditsUsed += 0.5;
                    if (creditsUsed > 0 && creditsUsed < 1) creditsUsed = 1;
                    account.credits -= (int)creditsUsed;
                    CreditsSystem.SaveFile();
                    if (account.credits <= 0)
                    {
                        if (user.Roles.Contains(CommandHandler.AIUserRole))
                            await user.RemoveRoleAsync(CommandHandler.AIUserRole);
                    }
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithAuthor($"ChatGPT Request", Context.Guild.IconUrl);
                    embed.WithThumbnailUrl(user.GetAvatarUrl());
                    embed.WithColor(36, 122, 191);
                    embed.WithFields(new EmbedFieldBuilder[] { new EmbedFieldBuilder().WithIsInline(true).WithName("Credits Used").WithValue((int)creditsUsed),
                        new EmbedFieldBuilder().WithIsInline(true).WithName("Remaining Credits").WithValue(account.credits)
                    });
                    embed.WithDescription(prompt);
                    //string msg = $"> {Context.User.Mention} *{(int)creditsUsed} credits used | {account.credits} credits remaining*" +
                    //    $"\n> **Prompt:** __{prompt}__";
                    string responseText = response.choices[0].message.content;
                    int maxLength = 1500;
                    await Context.Channel.SendMessageAsync(Context.User.Mention, embed: embed.Build());
                    if (responseText.Length > maxLength)
                    {
                        // Separate by every word and new line
                        string[] words = responseText.Split(' ');
                        string res = "";
                        for (int i = 0; i < words.Length; i++)
                        {
                            // result + word > max length
                            if (res.Length > maxLength)
                            { // Send the message now and set the result to the word
                                await Context.Channel.SendMessageAsync(res);
                                res = words[i];
                            } // Add a space if isnt first word
                            else res += i == 0 ? words[i] : " " + words[i];
                        }
                        await Context.Channel.SendMessageAsync(res);
                        /*
                        int count = (responseText.Length / maxLength);
                        if (responseText.Length % maxLength != 0) count++;
                        for (int i = 0; i < count; i++)
                        {
                            string text = responseText.Substring(i * maxLength);
                            if (text.Length > maxLength) text = text.Substring(0, maxLength);
                            await Context.Channel.SendMessageAsync(text);
                        }
                        */
                    } else await Context.Channel.SendMessageAsync(responseText);
                }
                catch (Exception e)
                {
                    Utility.printERROR(e.Message);
                    await Context.Channel.SendMessageAsync(user.Mention +
                        $"\n> **_Prompt failed!_**\n> **{e.Message}**");
                }
            });

            Thread t = new Thread(threadStart);
            t.Start();
        }
    }
}
