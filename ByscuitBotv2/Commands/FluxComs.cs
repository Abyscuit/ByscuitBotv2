using ByscuitBotv2.Data;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ByscuitBotv2.Modules.BFL;
using ByscuitBotv2.Modules;
using System.IO;
using System.ComponentModel;
using byscuitBot;
using Discord;

namespace ByscuitBotv2.Commands
{
    public class FluxComs : ModuleBase<SocketCommandContext>
    {
        [Command("Flux")]
        [Alias("FluxGenerate", "FluxGen", "FluxPro")]
        [Summary("Create an AI Generated image using AI (1 credit ($0.04) each use) - Usage: {0}Flux <prompt>")]
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
                    Console.WriteLine(await Flux.GenerateImage(prompt));
                    
                    using (WebClient client = new WebClient())
                    {
                        // TODO: Loop for amount of images
                        // save the image downloaded count and paths in an array
                        // send all images at the end
                        string imageUrl = await Flux.GenerateImage(prompt);
                        string imgName = "AI-image" + new Random((int)DateTime.Now.Ticks).Next(0, int.MaxValue);
                        string tempPath = Directory.GetCurrentDirectory() + $"/AI-Images/";
                        string fullImgName = $"{imgName}.png";
                        string fullPath = tempPath + fullImgName;
                        if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                        Utility.printConsole("Fullpath to AI Image: " + fullPath);
                        client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                        {
                            account.credits -= 2;
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
    }
}
