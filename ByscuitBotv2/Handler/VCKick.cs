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
        public SocketUser Initiator { get; set; }
        public SocketUser Target { get; set; }
        public static int VotesNeeded, YesVotes = 0, NoVotes = 0;
        public static IUserMessage[] DirectMessages;
        public static List<IUserMessage> VotedMessages = new List<IUserMessage>();
        public TimeSpan TimeOutTime; //60secs, 5mins, 10mins, 1hour, 1day, 1week
        public static DateTime Expiration;
        public string Reason = "";
        public VCKick(SocketGuildUser initiator, SocketGuildUser target, string reason, int UserCount)
        {
            PermComs.VOTE_IN_PROGRESS = true;
            Initiator = initiator;
            Target = target;
            Reason = reason;
            VotesNeeded = UserCount;
            TimeOutTime = TimeSpan.FromMinutes(1);
            Expiration = DateTime.Now.AddMinutes(1);
        }

        public void ProcessVote(string respose)
        {
            if (respose.ToLower()[0] == 'y')
            {

            }
            else if(respose.ToLower()[0] == 'n')
            {

            }
        }

        public Embed CreatePrivateMessage()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle($"A Vote Kick Has Been Started For {Target.Username}")
                .WithDescription($"Reason: {Reason}\n\nVote by reacting to this message.\nExpires: <t:{Expiration.ToUnixTimestamp()}:R>")
                .WithCurrentTimestamp();

            return embed.Build();
        }

        public void SetDirectMessages(IUserMessage[] directMessages)
        {
            DirectMessages = directMessages;
        }
    }
}
