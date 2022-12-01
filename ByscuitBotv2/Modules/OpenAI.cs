using ByscuitBotv2.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules
{
    public class OpenAI
    {
        private static string API_KEY = Program.config.OPENAI_API_KEY;
        private static string API_IMG_GEN = "https://api.openai.com/v1/images/generations";
        private static string API_IMG_EDIT = "https://api.openai.com/v1/images/edits";
        struct Response
        {
            public uint created;
            public ResponseData[] data;
        }
        struct ResponseData
        {
            public string url;
        }
        // sizes 256x256, 512x512, or 1024x1024
        public static string createImage(string prompt, string numberOfImgs, string size)
        {
            if (String.IsNullOrEmpty(prompt) ||
                String.IsNullOrEmpty(numberOfImgs) ||
                String.IsNullOrEmpty(size))
            {
                Utility.printERROR("Open.AI Image Generation missing parameter");
                return "Missing parameter";
            }

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(API_IMG_GEN);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Authorization", $"Bearer {API_KEY}");

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = $"{{\"prompt\":\"{prompt}\"," +
                              $"\"n\":{numberOfImgs}," +
                              $"\"size\":\"{size}\"}}";
                Utility.printConsole("json: " + json);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            Response response = JsonConvert.DeserializeObject<Response>(result);
            Utility.printConsole($"Open AI response: ${response.data[0].url}");
            return response.data[0].url;
        }
    }
}
