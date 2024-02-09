using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules.OpenAI
{
    public class GPT_3
    {
        private static string API_KEY = Config.API_KEY;
        private static string API_COMPLETIONS = Config.API_COMPLETIONS;

        public struct ResponseUsage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
        /// <summary>
        /// result: {"id":"chatcmpl-6ueFoargTHvGhEAhtPaNd2EMCaysu","object":"chat.completion",
        /// "created":1678959124,"model":"gpt-3.5-turbo-0301","usage":{"prompt_tokens":32,"completion_tokens":551,"total_tokens":583},
        /// "choices":[{"message":{"role":"assistant","content":""},"finish_reason":"stop","index":0}]}
        /// </summary>
        public struct ResponseChoice
        {
            //public string text;
            public ResponseMessage message;
            public int index;
            public int? logprobs;
            public string finish_reason;
        }
        public struct ResponseMessage
        {
            public string role;
            public string content;
        }
        public class Response
        {
            public ResponseChoice[] choices;
            public ResponseUsage usage;

            public Response(ResponseChoice[] Choices, ResponseUsage Usage)
            {
                choices = Choices;
                usage = Usage;
            }
        }
        struct ResponseError
        {
            public string code;
            public string message;
            public string param;
            public string type;
        }
        public static async Task<Response> CreateCompletion(string prompt)
        {
            if (String.IsNullOrEmpty(prompt))
            {
                Utility.printERROR("Open.AI Moderation missing parameter");
                throw new Exception("Missing Parameter");
            }
            string escapedInput = JsonConvert.SerializeObject(prompt); //prompt.Replace("\"", "\\\"");
            Console.WriteLine("Prompt:\n" + escapedInput);
            string model = "gpt-4";
            //int maxTokens = 3000;
            //string json = $"{{\"model\":\"{model}\",\"prompt\":\"{escapedInput}\"," +
            //        $"\"max_tokens\":{maxTokens},\"stream\":false}}";
            //string json = $"{{\"model\":\"{model}\",\"messages\":[" +
            //        $"{{\"role\":\"user\",\"content\":{escapedInput}}}]," +
            //        $"\"max_tokens\":{maxTokens},\"stream\":false}}";
            string json = $"{{\"model\":\"{model}\",\"messages\":[" +
                    $"{{\"role\":\"user\",\"content\":{escapedInput}}}]," +
                    $"\"stream\":false}}";
            Utility.printConsole("json: " + json);
            var jsonContent = new StringContent(json);
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(30);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
            var httpResponse = await httpClient.PostAsync(API_COMPLETIONS, jsonContent);
            string result = await httpResponse.Content.ReadAsStringAsync();
            Utility.printConsole("result: " + result);
            var response = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
            if (response.ContainsKey("error"))
            {
                ResponseError error = (ResponseError)response["error"];
                throw new Exception(error.message);
            }
            string choicesStr = JsonConvert.SerializeObject(response["choices"]);
            string usageStr = JsonConvert.SerializeObject(response["usage"]);
            ResponseChoice[] choices = JsonConvert.DeserializeObject<ResponseChoice[]>(choicesStr);
            ResponseUsage usage = JsonConvert.DeserializeObject<ResponseUsage>(usageStr);
            Utility.printConsole($"Open AI response: {JsonConvert.SerializeObject(choices)}");
            Utility.printConsole($"Open AI Usage: {JsonConvert.SerializeObject(usage)}");
            Response res = new Response(choices, usage);
            return res;
        }
    }
}
