using ByscuitBotv2.Modules;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Data
{
    public class Roles
    {
        public class Role
        {
            public ulong RoleID;
            public int Hours;

            public bool isSame(Role role)
            {
                if (RoleID == role.RoleID) return true;
                return false;
            }
        }

        public static List<Role> roles = new List<Role>();

        static string path = "Resources/";
        static string file = "Roles.json";
        static string fullpath = path + file;

        public static void Save()
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            File.WriteAllText(fullpath, JsonConvert.SerializeObject(roles, Formatting.Indented));
            string text = $"Saved set roles: {fullpath}";
            Utility.printConsole(text);
        }

        public static void Load()
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!File.Exists(fullpath))
            {
                File.WriteAllText(fullpath, JsonConvert.SerializeObject(roles, Formatting.Indented));
                string text = $"Generated new file for roles: {fullpath}";
                Utility.printConsole(text);
                text = $"Use the AddRole command to add roles to the config file.";
                Utility.printConsole(text);
            }
            else
            {
                string contents = File.ReadAllText(fullpath);
                roles = JsonConvert.DeserializeObject<List<Role>>(contents);
            }
        }

        public static List<Role> CheckRoles(int hoursSpent)
        {
            List<Role> earnedRoles = new List<Role>();
            foreach (Role role in roles) if (hoursSpent >= role.Hours) earnedRoles.Add(role);

            return earnedRoles;
        }

        public static Role GetRole(ulong roleID)
        {
            Role result = null;
            foreach(Role r in roles)
            {
                if(r.RoleID == roleID)
                {
                    result = r;
                    break;
                }
            }
            return result;
        }

        public static void AddRole(SocketRole role, int hours)
        {
            Role r = new Role();
            r.RoleID = role.Id;
            r.Hours = hours;
            roles.Add(r);
            Save();
        }

        public static bool RemoveRole(SocketRole role)
        {
            bool result = roles.Remove(Roles.GetRole(role.Id));
            Save();
            return result;
        }
    }
}
