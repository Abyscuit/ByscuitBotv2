using ByscuitBotv2.Handler;
using ByscuitBotv2.Modules;
using Discord;
using Discord.Commands;
using Discord.Rest;
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
        public static RestUserMessage VOTE_MESSAGE = null;
        [Command("VCKick", RunMode = RunMode.Async)]
        [Alias("voicekick", "kickvc", "votekick")]
        [Summary("Starts a timeout vote from for a user with an optional reason - Usage: {0}vckick <user> <reason>")]
        public async Task VCKick(SocketGuildUser Target, [Remainder] string text = "")
        {
            if (Context.User.Id == Target.Id)
            {
                await Context.Channel.SendMessageAsync("> You can't start a vote kick against yourself!");
                return;
            }
            if (Target.IsBot)
            {
                await Context.Channel.SendMessageAsync("> You can't start a vote kick against a bot!");
                return;
            }
            if (Target.VoiceChannel== null)
            {
                await Context.Channel.SendMessageAsync("> You can't start a vote kick against someone not in a voice channel!");
                return;
            }
            RequestOptions deleteOptions = RequestOptions.Default;
            deleteOptions.AuditLogReason = "Delete vote message";
            await Context.Message.DeleteAsync(deleteOptions);

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
            int minUsers = 4;
            if (UsersInChat.Length < minUsers)
            {
                await Context.Channel.SendMessageAsync($"> There must be at least {minUsers} people in the voice channel to start a vote kick!");
                return;
            }
            int deduction = 2;
            if (Target.IsSelfDeafened || Target.IsDeafened) deduction = 1;
            Handler.VCKick.StartVote(Initiator, Target, text, UsersInChat.Length - deduction);
            // Make sure there is at least 5 people minus the Initiator and Target.
            List<IUserMessage> Messages = new List<IUserMessage>();
            for(int i =0;i<UsersInChat.Length;i++) {
                ulong UserID = UsersInChat[i].Id;
                if (UserID != Target.Id && UserID != Initiator.Id)
                {
                    IUserMessage message = Utility.DirectMessage(UsersInChat[i], embed: Handler.VCKick.CreatePrivateMessage()).GetAwaiter().GetResult();
                    await message.AddReactionsAsync(Handler.VCKick.EMOJIS);
                    Console.WriteLine("Channel add: " + message.Id);
                    Messages.Add(message);
                }
            }
            Console.WriteLine("Channels: " + Messages.Count);
            Handler.VCKick.DirectMessages = Messages.ToArray();
            VOTE_MESSAGE = await Context.Channel.SendMessageAsync(embed: Handler.VCKick.CreatePublicMessage());
        }
    }
}
