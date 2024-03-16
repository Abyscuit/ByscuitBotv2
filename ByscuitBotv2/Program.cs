using byscuitBot;
using ByscuitBotv2.Data;
using ByscuitBotv2.Modules;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ByscuitBotv2
{
    class Program
    {
        public static DiscordSocketClient client;
        public static string game = "for commands";
        CommandHandler handler;
        private IServiceProvider services;
        public static Config config;
        static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

    
        public async Task StartAsync()
        {
            await SetupClient();
            await Task.Delay(-1);
        }
        delegate void formCloseCallback();
        public async Task SetupClient()
        {
            //if (t.IsAlive)
            {
                //if(form.InvokeRequired)
                {
                    //formCloseCallback formCallBack = new formCloseCallback(form.Close);
                    //form.Invoke(formCallBack);
                }
                //else form.Close();
            }

            config = Config.LoadConfig();// Load that config!

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = config.DEBUG_LEVEL, //change log level in config file
                ConnectionTimeout = 30000, // 30 second timeout
                DefaultRetryMode = RetryMode.AlwaysRetry,
                AlwaysDownloadUsers = true, // Enable Server Member Intent in Discord API Settings
                GatewayIntents = GatewayIntents.All
            });

            

            client.Log += Client_Log;
            await client.SetGameAsync(game, type: ActivityType.Watching);
            await client.LoginAsync(Discord.TokenType.Bot, config.DISCORD_API_KEY);
            await client.StartAsync();
            
            IServiceCollection serviceCollection = new ServiceCollection();
            services = serviceCollection.BuildServiceProvider();

            handler = new CommandHandler();
            await handler.InitializeAsync(client);
        }

        private async Task Client_Log(Discord.LogMessage msg)
        {
            await consolePrint(msg.Message);
            if (msg.Message == "Failed to resume previous session")
                await consolePrint("Attempting to create new session...");
            // Create a main log file?
            // Error log file only too?
        }

        public async Task consolePrint(string msg)
        {
            Utility.printConsole(msg);
            await Task.CompletedTask;
        }
    }
}
