using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules.OpenAI
{
    public class Config
    {
        public static string API_KEY = Program.config.OPENAI_API_KEY;
        public static string API_IMG_GEN = "https://api.openai.com/v1/images/generations";
        public static string API_IMG_EDIT = "https://api.openai.com/v1/images/edits";
        public static string API_MODERATION = "https://api.openai.com/v1/moderations";
        public static string API_COMPLETIONS = "https://api.openai.com/v1/chat/completions";
    }
}
