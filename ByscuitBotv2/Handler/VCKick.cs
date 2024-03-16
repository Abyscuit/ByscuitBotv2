using ByscuitBotv2.Commands;
using Discord;
using Discord.WebSocket;
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
        public int VotesNeeded, YesVotes = 0, NoVotes = 0;
        public IUserMessage[] DirectMessages;
        public ulong[] VoteMessage;
        public TimeSpan TimeOutTime; //60secs, 5mins, 10mins, 1hour, 1day, 1week
        public string Reason = "";
        public VCKick()
        {
            PermComs.VOTE_IN_PROGRESS = true;
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
                .WithTitle($"A Vote Kick Has Been Started")
                .WithDescription("")
                .WithCurrentTimestamp();

            return embed.Build();
        }
    }
}
