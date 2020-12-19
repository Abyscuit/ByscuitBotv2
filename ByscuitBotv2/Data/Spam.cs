using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Data
{
    public class Spam
    {
        public long discordID;
        public string username;
        public uint discriminator;// Username # number
        public ulong[] lastMessages;// Timestamp of last messages
        public Warning[] warnings;// Storage for warnings

        public struct Warning
        {
            public ulong time;// current time of warning
            public uint cWarns;// Number of consecutive warnings
        }
    }
}
