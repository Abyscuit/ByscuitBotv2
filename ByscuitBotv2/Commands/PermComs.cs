using ByscuitBotv2.Handler;
using ByscuitBotv2.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Commands
{
    public class PermComs : ModuleBase<SocketCommandContext>
    {
        public static bool VOTE_IN_PROGRESS = false;
        [Command("VCKick")]
        [Alias("voicekick", "kickvc", "votekick")]
        [Summary("Starts a timeout vote from for a user with an optional reason - Usage: {0}vckick <user> <reason>")]
        public async Task VCKick(SocketGuildUser Target, [Remainder] string text = "")
        {
            SocketGuildUser Initiator = Context.User as SocketGuildUser;
            if (Initiator.VoiceChannel == null)
            {
                await Context.Channel.SendMessageAsync("> You must be in a *Voice Channel* to use this command!");
                return;
            }
            if (VOTE_IN_PROGRESS)
            {
                await Context.Channel.SendMessageAsync("> There is already a vote in progress!");
                return; 
            }
            if (!Target.VoiceChannel.ConnectedUsers.Contains(Initiator))
            {
                await Context.Channel.SendMessageAsync("> You must be in the same voice channel to kick the user!");
                return;
            }
            SocketGuildUser[] UsersInChat = Utility.GetUndefeanedUsersFromChannel(Target.VoiceChannel);
            VCKick VoteKick = new VCKick(Initiator, Target, text, UsersInChat.Length - 2);
            // Make sure there is at least 5 people minus the Initiator and Target.
            List<IUserMessage> Messages = new List<IUserMessage>();
            for(int i =0;i<UsersInChat.Length;i++) {
                ulong UserID = UsersInChat[i].Id;
                if (UserID != Target.Id && UserID != Initiator.Id)
                {
                    IUserMessage message = Utility.DirectMessage(UsersInChat[i], embed: VoteKick.CreatePrivateMessage()).GetAwaiter().GetResult();
                    var YesEmoji = new Emoji("✅");
                    var NoEmoji = new Emoji("❌");
                    Emoji[] emojis = {YesEmoji, NoEmoji};
                    await message.AddReactionsAsync(emojis);
                    Console.WriteLine("Channel add: " + message.Id);
                    Messages.Add(message);
                }
            }
            Console.WriteLine("Channels: " + Messages.Count);
            RequestOptions options = RequestOptions.Default;
            options.AuditLogReason = text;
            //await Target.SetTimeOutAsync(TimeSpan.FromSeconds(1), options);
        }
    }
}
