using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.Internal;
using ByscuitBotv2.Util;

namespace ByscuitBotv2.Modules
{
    public class CsgoBot
    {

        public SteamAccount JsonAccount;

        public Dictionary<string, string> Cookies = new Dictionary<string, string>();

        internal CsgoBot(SteamAccount json)
        {
            JsonAccount = json;
        }

        ~CsgoBot()
        {
            if (IsRunning)
            {
                Stop();
            }
        }

        ////////////////////////////////////////////////////
        // TIME
        ////////////////////////////////////////////////////

        public long StartEpoch { get; set; }

        public bool IsRunning { get; set; }

        public bool IsAuthenticated { get; set; }

        ////////////////////////////////////////////////////
        // GENERAL
        ////////////////////////////////////////////////////

        public abstract Result Start();
        public abstract void Stop();

        public abstract void OnConnected(SteamClient.ConnectedCallback callback);
        public abstract void OnDisconnected(SteamClient.DisconnectedCallback callback);

        public abstract void OnLoggedOn(SteamUser.LoggedOnCallback callback);
        public abstract void OnLoggedOff(SteamUser.LoggedOffCallback callback);

        ////////////////////////////////////////////////////
        // STEAM WEB INTERFACE
        ////////////////////////////////////////////////////

        public abstract void LoginWebInterface(ulong steamID);

        public abstract void JoinSteamGroup(uint groupID = 28495194); // 28495194 is /groups/TitanReportBot

        ////////////////////////////////////////////////////
        // GAME COORDINATOR
        ////////////////////////////////////////////////////

        public void OnGCMessage(SteamGameCoordinator.MessageCallback callback)
        {
            var map = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { (uint) EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientReportResponse, OnReportResponse },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchmakingGC2ClientHello, OnMatchmakingHelloResponse },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientCommendPlayerQueryResponse, OnCommendResponse },
                { (uint) ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchList, OnLiveGameRequestResponse }
            };

            if (map.TryGetValue(callback.EMsg, out var func))
            {
                func(callback.Message);
            }
        }

        public abstract void OnFreeLicenseResponse(ClientMsgProtobuf<CMsgClientRequestFreeLicenseResponse> action);

        public abstract void OnClientWelcome(IPacketGCMsg msg);
        public abstract void OnMatchmakingHelloResponse(IPacketGCMsg msg);
        public abstract void OnReportResponse(IPacketGCMsg msg);
        public abstract void OnCommendResponse(IPacketGCMsg msg);
        public abstract void OnLiveGameRequestResponse(IPacketGCMsg msg);

        ////////////////////////////////////////////////////
        // BOTTING SESSION INFORMATIONS
        ////////////////////////////////////////////////////

        public ReportInfo _reportInfo; // Setting this to private will cause it not to be visible for inheritated classes

        public void FeedReportInfo(ReportInfo info)
        {
            _reportInfo = info;
        }

        public CommendInfo _commendInfo; // Setting this to private will cause it not to be visible for inheritated classes

        public void FeedCommendInfo(CommendInfo info)
        {
            _commendInfo = info;
        }

        public LiveGameInfo _liveGameInfo; // Setting this to private will cause it not to be visible for inheritated classes
        public MatchInfo MatchInfo;

        public void FeedLiveGameInfo(LiveGameInfo info)
        {
            _liveGameInfo = info;
        }

        public uint GetTargetAccountID()
        {
            if (_reportInfo != null)
            {
                return _reportInfo.SteamID.AccountID;
            }

            if (_commendInfo != null)
            {
                return _commendInfo.SteamID.AccountID;
            }

            if (_liveGameInfo != null)
            {
                return _liveGameInfo.SteamID.AccountID;
            }

            return 0;
        }

        public abstract uint GetReporterAccountID();


        public IClientGCMsg GetReportPayload(int payloadID = 0)
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientReportPlayer>(
                (uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientReportPlayer
            )
            {
                Body =
                        {
                            account_id = _reportInfo.SteamID.AccountID,
                            match_id = _reportInfo.MatchID,

                            rpt_aimbot = Convert.ToUInt32(_reportInfo.AimHacking),
                            rpt_wallhack = Convert.ToUInt32(_reportInfo.WallHacking),
                            rpt_speedhack = Convert.ToUInt32(_reportInfo.OtherHacking),
                            rpt_teamharm = Convert.ToUInt32(_reportInfo.Griefing),
                            rpt_textabuse = Convert.ToUInt32(_reportInfo.AbusiveText),
                            rpt_voiceabuse = Convert.ToUInt32(_reportInfo.AbusiveVoice)
                        }
            };

            return payload;
        }


        public IClientGCMsg GetCommendPayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientCommendPlayer>(
                (uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientCommendPlayer
            )
            {
                Body =
                {
                    account_id = _commendInfo.SteamID.AccountID,
                    match_id = 0,
                    commendation = new PlayerCommendationInfo
                    {
                        cmd_friendly = Convert.ToUInt32(_commendInfo.Friendly),
                        cmd_teaching = Convert.ToUInt32(_commendInfo.Teacher),
                        cmd_leader = Convert.ToUInt32(_commendInfo.Leader)
                    },
                    tokens = 0
                }
            };

            return payload;
        }

        public IClientGCMsg GetLiveGamePayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchListRequestLiveGameForUser>(
                (uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchListRequestLiveGameForUser
            )
            {
                Body =
                {
                    accountid = _liveGameInfo.SteamID.AccountID
                }
            };

            return payload;
        }

        public IClientGCMsg GetRequestPlayerProfilePayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_ClientRequestPlayersProfile>(
                (uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_ClientRequestPlayersProfile
            )
            {
                Body =
                {
                    account_id = GetTargetAccountID(),
                    request_level = 1 // ???
                }
            };

            return payload;
        }

        public IClientGCMsg GetClientGamesPlayedPayload()
        {
            var payload = new ClientGCMsgProtobuf<CMsgClientGamesPlayed>(
                (uint)EMsg.ClientGamesPlayed
            )
            {
                Body =
                {
                    games_played =
                    {
                        new CMsgClientGamesPlayed.GamePlayed
                        {
                            steam_id_gs = _reportInfo.GameServerID,
                            game_id = 730,
                            game_ip_address = new CMsgIPAddress(){v4 = 0},
                            game_port = 0,
                            game_extra_info = "Counter - Strike: Global Offensive",
                            process_id = RandomUtil.RandomUInt32(),
                            game_flags = 1,
                            streaming_provider_id = 0,
                            owner_id = GetReporterAccountID()
                        }
                    }
                }
            };

            return payload;
        }

    }
}
