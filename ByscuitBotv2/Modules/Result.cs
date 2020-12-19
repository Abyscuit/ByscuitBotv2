using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules
{
    public enum Result
    {
        Success,
        AlreadyLoggedInSomewhereElse,
        AccountBanned,
        TimedOut,
        SentryRequired,
        RateLimit,
        NoMatches,
        Code2FAWrong,
        NoGame,
        Unknown
    }
}
