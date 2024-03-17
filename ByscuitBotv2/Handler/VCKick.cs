using ByscuitBotv2.Commands;
using Discord;
using Discord.WebSocket;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Handler
{
    public class VCKick
    {
        public static SocketGuildUser Initiator { get; set; }
        public static SocketGuildUser Target { get; set; }
        public static int VotesNeeded, YesVotes = 0, NoVotes = 0;
        public static IUserMessage[] DirectMessages;
        public static List<IUserMessage> VotedMessages = new List<IUserMessage>();
        public static TimeSpan TimeOutTime = TimeSpan.FromSeconds(60); //60secs, 5mins, 10mins, 1hour, 1day, 1week
        public static DateTimeOffset Expiration;
        public static string Reason = "";
        public static Emoji YES_EMOJI = new Emoji("✅");
        public static Emoji NO_EMOJI = new Emoji("❌");
        public static Emoji[] EMOJIS = { YES_EMOJI, NO_EMOJI };
        public static void StartVote(SocketGuildUser initiator, SocketGuildUser target, string reason, int UserCount)
        {
            PermComs.VOTE_IN_PROGRESS = true;
            Initiator = initiator;
            Target = target;
            Reason = reason;
            VotesNeeded = UserCount;
            YesVotes = 1;
            NoVotes = 1;
            VotedMessages = new List<IUserMessage>();
            TimeOutTime = TimeSpan.FromMinutes(1); 

            DateTime currentTime = DateTime.UtcNow;
            Expiration = ((DateTimeOffset)currentTime).AddMinutes(1);
        }

        public static async void ProcessVote(IEmote vote)
        {
            if (vote.Name == YES_EMOJI.Name) YesVotes++;
            else if (vote.Name == NO_EMOJI.Name) NoVotes++;

            if(VotedMessages.Count >= VotesNeeded)
            {
                // Check if yes votes win
                Console.WriteLine("Yes: " + YesVotes);
                Console.WriteLine("Needed: " + VotesNeeded);
                Console.WriteLine("VotedMessages.Count: " + VotedMessages.Count);
                if (YesVotes - 1 >= VotesNeeded)
                {
                    RequestOptions options = RequestOptions.Default;
                    options.AuditLogReason = Reason;
                    await Target.SetTimeOutAsync(TimeOutTime, options);
                }

                // Change embed to reflect votes and outcome
                await PermComs.VOTE_MESSAGE.ModifyAsync(m =>
                {
                    m.Embed = CreateCompletedMessage();
                });
                PermComs.VOTE_IN_PROGRESS = false;
            }
            else
            {
                await PermComs.VOTE_MESSAGE.ModifyAsync(m =>
                {
                    m.Embed = CreatePublicMessage();
                });
            }
        }

        public static Embed CreatePrivateMessage()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle($"A Vote Kick Has Been Started For {Target.Username}")
                .WithDescription($"Reason: {Reason}\n\nVote by reacting to this message.\nExpires: <t:{Expiration.ToUnixTimeSeconds()}:R>")
                .WithCurrentTimestamp();

            return embed.Build();
        }
        public static Embed CreatePublicMessage()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle($"A Vote Kick Has Been Started For {Target.Username}")
                .WithDescription($"Reason: {Reason}\n\nVotes: {VotedMessages.Count + 2}/{VotesNeeded + 2}\nExpires: <t:{Expiration.ToUnixTimeSeconds()}:R>")
                .WithFields(
                    new EmbedFieldBuilder() { IsInline = true, Name = "Yes", Value = YesVotes },
                    new EmbedFieldBuilder() { IsInline = true, Name = "No", Value = NoVotes })
                .WithCurrentTimestamp();

            return embed.Build();
        }
        public static Embed CreateCompletedMessage()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle($"Vote Kick Has Ended For {Target.Username}")
                .WithDescription($"Reason: {Reason}\n\nVotes: {VotedMessages.Count + 2}/{VotesNeeded + 2}\n" + 
                    $"User was {((YesVotes - 1 >= VotesNeeded) ? $"timed out for {TimeOutTime.TotalSeconds}secs" : "not timed out")}")
                .WithCurrentTimestamp();

            return embed.Build();
        }

        public void SetDirectMessages(IUserMessage[] directMessages)
        {
            DirectMessages = directMessages;
        }

        public static IUserMessage CheckMsgInVote(ulong MessageID)
        {
            for(int i = 0; i < DirectMessages.Length; i++)
            {
                if (DirectMessages[i].Id == MessageID) return DirectMessages[i];
            }
            return null;
        }
        public static bool CheckMsgAlreadyVoted(ulong MessageID)
        {
            for (int i = 0; i < VotedMessages.Count; i++)
            {
                if (VotedMessages[i].Id == MessageID) return true;
            }
            return false;
        }
    }
}
