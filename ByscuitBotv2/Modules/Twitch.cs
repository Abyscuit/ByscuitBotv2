using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules
{
    public class Twitch
    {
        // Make web request to twitch channel
        // check if json response is null
        /*
          {
            "broadcaster_language": "en",
            "display_name": "a_seagull",
            "game_id": "506442",
            "id": "19070311",
            "is_live": true,
            "tags_ids": [
                "6ea6bca4-4712-4ab9-a906-e3336a9d8039"
            ],
            "thumbnail_url": "https://static-cdn.jtvnw.net/jtv_user_pictures/a_seagull-profile_image-4d2d235688c7dc66-300x300.png",
            "title": "a_seagull",
            "started_at": "2020-03-18T17:56:00Z"
           }
        */
        public class ChannelData
        {
            public string broadcaster_language;
            public string display_name;
            public string game_id;
            public string id;
            public bool is_live;
            public string[] tags_ids;
            public string thumbnail_url;
            public string title;
            public string started_at;
        }

        public class Response
        {

            public ChannelData[] data;
            public struct Pagination
            {

            }
        }

        public static string ACCESS_TOKEN = "";
        public static string GET_ACCESS_TOKEN()
        {
            if(ACCESS_TOKEN == "")
            {

            }
            return ACCESS_TOKEN;
        }
    }
}
