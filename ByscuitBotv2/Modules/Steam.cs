using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.Internal;

namespace ByscuitBotv2.Modules
{
    public class Steam
    {
        SteamClient steamClient = new SteamClient();
        CallbackManager manager;
        public SteamUser steamUser;
        SteamFriends steamFriends;
        SteamApps steamApps;
        bool isRunning;
        bool bAborted;
        int seq; // more hack fixes
        bool bConnected;
        bool bConnecting;
        bool bDidDisconnect;
        public Dictionary<uint, ulong> AppTokens { get; private set; }
        DateTime connectTime;
        static readonly TimeSpan STEAM3_TIMEOUT = TimeSpan.FromSeconds(30);
        public string user, pass, personaname;
        public bool hasVac, hasBan;
        string[] Ranks =
        {
            "Unranked",
            "Silver1",
            "Silver2",
            "Silver3",
            "Silver4",
            "Silver Elite",
            "Silver Elite Master", 
            "GOLD NOVA 1",
            "GOLD NOVA 2",
            "GOLD NOVA 3",
            "GOLD NOVA Master",
            "MG1",
            "MG2", 
            "MGE",
            "DMG",
            "LE",
            "LEM",
            "SMFC",
            "GE"
        };
        public Dictionary<uint, SteamApps.PICSProductInfoCallback.PICSProductInfo> AppInfo { get; private set; }
        public Dictionary<uint, SteamApps.PICSProductInfoCallback.PICSProductInfo> PackageInfo { get; private set; }

        public Steam()
        {
            steamClient = new SteamClient();
            manager = new CallbackManager(steamClient);
        }

        public void Login(string User, string Pass)
        {
            user = User;
            pass = Pass;
            bAborted = false;
            this.bConnected = false;
            this.bConnecting = false;
            this.seq = 0;
            this.AppInfo = new Dictionary<uint, SteamApps.PICSProductInfoCallback.PICSProductInfo>();
            this.PackageInfo = new Dictionary<uint, SteamApps.PICSProductInfoCallback.PICSProductInfo>();
            steamClient = new SteamClient();
            manager = new CallbackManager(steamClient);
            steamUser = steamClient.GetHandler<SteamUser>();
            steamFriends = steamClient.GetHandler<SteamFriends>();
            this.steamApps = this.steamClient.GetHandler<SteamApps>();
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            manager.Subscribe<SteamApps.LicenseListCallback>(OnLicenseList);
            isRunning = true;
            Console.WriteLine("Connecting to Steam...");

            bAborted = false;
            bConnected = false;
            bConnecting = true;
            bDidDisconnect = false;
            this.connectTime = DateTime.Now;

            // initiate the connection
            steamClient.Connect();

            // create our callback handling loop
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from Steam");

            isRunning = false;
        }

        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Console.WriteLine("Connected to Steam! Logging in '{0}'...", user);

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,
            });
        }
        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {
                    // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                    // then the account we're logging into is SteamGuard protected
                    // see sample 5 for how SteamGuard can be handled

                    Console.WriteLine("Unable to logon to Steam: This account is SteamGuard protected.");
                    
                    isRunning = false;
                    return;
                }

                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                isRunning = false;
                return;
            }

            Console.WriteLine("Successfully logged on!");
            ulong steamID = steamUser.SteamID.ConvertToUInt64();
            personaname = steamFriends.GetPersonaName();
            Console.WriteLine("Username: {0}", personaname);

            // for WebAPIs that require an API key, the key can be specified in the GetInterface function
            using (dynamic steamUserAuth = WebAPI.GetInterface("ISteamUser", "0929AB04644BF6B142E30588D8E54A1F"))
            {
                // as the interface functions are synchronous, it may be beneficial to specify a timeout for calls
                steamUserAuth.Timeout = TimeSpan.FromSeconds(5);
                Dictionary<string, object> args = new Dictionary<string, object>();
                args["steamids"] = steamID;
                KeyValue results = steamUserAuth.Call("GetPlayerBans", 1, args);
                foreach (KeyValue bans in results["players"].Children)
                {
                    hasVac = bans["VACBanned"].AsBoolean();
                    hasBan = ((bans["NumberOfGameBans"].AsInteger() > 0) || bans["CommunityBan"].AsBoolean());
                    if (hasVac) Console.WriteLine("Has VAC Ban!");
                    if (hasBan) Console.WriteLine("Has Potential Game Ban!");
                    //Console.WriteLine("Community Ban: {0}", bans["CommunityBanned"].AsString());
                    //Console.WriteLine("VAC Ban: {0}", bans["VACBanned"].AsString());
                    //Console.WriteLine("Days Since Last Ban: {0}", bans["DaysSinceLastBan"].AsString());
                    //Console.WriteLine("Number of VAC Bans: {0}", bans["NumberOfVACBans"].AsString());
                    //Console.WriteLine("Number of Game Bans: {0}", bans["NumberOfGameBans"].AsString());
                    //Console.WriteLine("Economy Ban: {0}", bans["EconomyBan"].AsString());
                }
            }
            //steamUser.LogOff();
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }

        private void OnLicenseList(SteamApps.LicenseListCallback callback)
        {
            ReadOnlyCollection<SteamApps.LicenseListCallback.License> license = callback.LicenseList;
            if (license.Count == 0) return;
            List<uint> packages = new List<uint>();
            foreach(SteamApps.LicenseListCallback.License app in license) packages.Add(app.PackageID);
            bool completed = false;
            Action<SteamApps.PICSProductInfoCallback> cbMethod = (packageInfo) =>
            {
                completed = !packageInfo.ResponsePending;

                foreach (var package_value in packageInfo.Packages)
                {
                    var package = package_value.Value;
                    if (package.ID == 0) continue;// Basic steam free games included package, skip it
                    List<KeyValue> appIDs = package.KeyValues["appids"].Children;
                    for (int i = 0; i < appIDs.Count; i++)
                    {
                        string appId = appIDs[i].AsString(); 
                        //Console.WriteLine($"Found App ID {appId} in package!");
                        if (appId == "730") { Console.WriteLine("Found CSGO!"); steamUser.LogOff(); break; }
                    }
                    //CMsgClientGetUserStats stats = new CMsgClientGetUserStats();
                    //EMsg.ClientGetUserStats
                }
                Disconnect();
            };

            WaitUntilCallback(() =>
            {
                manager.Subscribe(steamApps.PICSGetProductInfo(new List<uint>(), packages), cbMethod);
            }, () => { return completed; });
        }
        public delegate bool WaitCondition();
        public bool WaitUntilCallback(Action submitter, WaitCondition waiter)
        {
            while (!bAborted && !waiter())
            {
                submitter();

                int seq = this.seq;
                do
                {
                    WaitForCallbacks();
                }
                while (!bAborted && this.seq == seq && !waiter());
            }

            return bAborted;
        }
        private void WaitForCallbacks()
        {
            manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));

            TimeSpan diff = DateTime.Now - connectTime;

            if (diff > STEAM3_TIMEOUT && !bConnected)
            {
                Console.WriteLine("Timeout connecting to Steam3.");
                Abort();

                return;
            }
        }
        private void Abort(bool sendLogOff = true)
        {
            Disconnect(sendLogOff);
        }
        public void Disconnect(bool sendLogOff = true)
        {
            if (sendLogOff)
            {
                steamUser.LogOff();
            }

            steamClient.Disconnect();
            bConnected = false;
            bConnecting = false;
            bAborted = true;
            isRunning = false;
            bDidDisconnect = true;

            // flush callbacks until our disconnected event
            while (!bDidDisconnect)
            {
                manager.RunWaitAllCallbacks(TimeSpan.FromMilliseconds(100));
            }
        }
        /* Finish the login function
         * then finish the ban detection and
         * CSGO level checker
         */
    }
}
