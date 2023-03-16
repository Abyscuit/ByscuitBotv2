using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules.OpenAI
{
    public class Moderation
    {
        private static string API_KEY = Config.API_KEY;
        private static string API_MODERATION = Config.API_MODERATION;

        /// <summary>
        /// Struct for moderation
        /// prompt text
        /// </summary>
        struct Response
        {
            public string id;
            public string model;
            public ResponseData[] results;
            public ResponseError? error;
        }
        struct ResponseData
        {
            public Dictionary<string, bool> categories;
            public Dictionary<string, decimal> category_scores;
            public bool flagged;
        }
        struct ResponseError
        {
            public string code;
            public string message;
            public string param;
            public string type;
        }


        public static async Task<bool> checkPrompt(string prompt)
        {
            if (String.IsNullOrEmpty(prompt))
            {
                Utility.printERROR("Open.AI Moderation missing parameter");
                throw new Exception("Missing Parameter");
            }
            string escapedInput = prompt.Replace("\"", "\\\"");
            string json = $"{{\"input\":\"{escapedInput}\"," +
                    $"\"model\":\"text-moderation-latest\"}}";
            Utility.printConsole("json: " + json);
            var jsonContent = new StringContent(json);
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
            var httpResponse = await httpClient.PostAsync(API_MODERATION, jsonContent);
            string result = await httpResponse.Content.ReadAsStringAsync();
            Utility.printConsole("result: " + result);
            Response response = JsonConvert.DeserializeObject<Response>(result);
            if (response.error != null) throw new Exception(response.error.Value.message);
            Utility.printConsole($"Open AI response: ${response.results}");
            return response.results[0].flagged;
        }
    }
}
