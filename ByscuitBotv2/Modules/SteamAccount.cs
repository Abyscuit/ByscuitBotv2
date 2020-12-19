using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules
{
    public class SteamAccount
    {
        public string Username;
        public uint Level;
        public string Login;
        public string Password;
        public DateTime CooldownStart;
        public DateTime CooldownEnd;

        public SteamAccount()
        {

        }
        public void AddCooldown(int days, int hours)
        {
            CooldownStart = DateTime.Now;
            CooldownEnd = CooldownStart.AddDays(days).AddHours(hours);
        }
    }
}
