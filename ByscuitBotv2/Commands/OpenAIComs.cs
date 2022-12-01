using byscuitBot;
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
        [Summary("Create an AI Generated image using Open.AI API ($0.04 each use) - Usage: {0}AIImage <prompt>")]
        public async Task AIGenerateImage([Remainder]string prompt)
        {
            SocketGuildUser user = (SocketGuildUser)Context.User;
            if (!(user.Roles.Contains(CommandHandler.PremiumByscuitRole) || user.Roles.Contains(CommandHandler.AIUserRole))
                && !user.GuildPermissions.Administrator)
            {
                await Context.Channel.SendMessageAsync("**Sorry you need to be a server booster to use this function!**");
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
                        string msg = $"> {Context.User.Mention}'s __**AI Generated Image**__\n> **Prompt:** {prompt}";
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
    }
}
