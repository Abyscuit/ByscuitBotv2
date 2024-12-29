using ByscuitBotv2.Modules.BFL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules.BFL
{
    public class Config
    {
        public static string API_KEY = Program.config.BFL_API_KEY;
        static string BFL_API = "https://api.bfl.ml";
        public static string FLEX_PRO = $"{BFL_API}/v1/flux-pro-1.1";
        public static string FLEX_PRO_ULTRA = $"{BFL_API}/v1/flux-pro-1.1-ultra";
        public static string GET_RESULT = $"{BFL_API}/v1/get_result?id=";
    }
}
