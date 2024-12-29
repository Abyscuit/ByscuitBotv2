using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition.Primitives;
using SteamKit2.CDN;
using System.Net;

namespace ByscuitBotv2.Modules.BFL
{
    public class Flux
    {
        private static string API_KEY = Config.API_KEY;
        private static string IMG_GEN = Config.FLEX_PRO;

        public interface IPostResponse
        {
            string id { get; set; }
        }

        public interface IGetResponse
        {
            string id { get; set; }
            string status { get; set; }
            ResultData? result { get; set; }
        }

        public class Error
        {
            public List<ErrorDetail> detail { get; set; }
            public string id { get; set; }
        }

        public class ErrorDetail
        {
            public List<object> loc { get; set; }
            public string msg { get; set; }
            public string type { get; set; }
        }

        public class Response : IPostResponse, IGetResponse
        {
            public string id { get; set; }
            public string status { get; set; }
            public ResultData? result { get; set; }
        }

        public struct ResultData
        {
             public string sample; // "https://delivery-eu1.bfl.ai/results/6c27730604b945369335d3d385f4369a/sample.jpeg?se=2024-12-29T05%3A39%3A37Z&sp=r&sv=2024-11-04&sr=b&rsct=image/jpeg&sig=yxrNs%2BOhPWjvnqm1lKTyQh%2Blhjlhv491Bz/s21UX65w%3D",
             public string prompt; // "fat black women dancin in an alley with trump",
             public uint seed; // 3392768630,
             public double start_time; // 1735450176.362204,
             public double end_time; // 1735450177.853745,
             public double duration; // 1.4915409088134766
        }

        public class ProPrompt
        {
            public string prompt { get; set; }
            public int? width { get; set; }
            public int? height { get; set; }
            public bool? prompt_upsampling { get; set; }
            public int? seed { get; set; }
            public int? safety_tolerance { get; set; }
            public string output_format { get; set; }
        }

        public static async Task<string> GenerateImage(string prompt)
        {
            var promptReq = new ProPrompt
            {
                prompt = prompt,
                width = 1024,
                height = 768,
                prompt_upsampling = false,
                safety_tolerance = 6,
                output_format = "jpeg"
            };
            Console.WriteLine(JsonConvert.SerializeObject(promptReq));

            var content = new StringContent(JsonConvert.SerializeObject(promptReq), Encoding.UTF8, "application/json");
            //var options = new HttpRequestMessage(HttpMethod.Post, new Uri(IMG_GEN))
            //{
            //    Content = new StringContent(promptReq.ToString(), Encoding.UTF8, "application/json")
            //};
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Key", API_KEY);
            var response = await client.PostAsync(IMG_GEN, content);
            var responseData = await response.Content.ReadAsStringAsync();
            var fluxResponse = JsonConvert.DeserializeObject<Response>(responseData);

            Console.WriteLine("response:\n" + responseData);

            int count = 0;
            const int timeout = 30;
            Response imgResponse = null;

            while (imgResponse == null)
            {
                await Task.Delay(500);
                imgResponse = await GetImageFromAPI(fluxResponse.id);
                Console.WriteLine("imgResponse:\n" + JsonConvert.SerializeObject(imgResponse));
                count++;
                if (count == timeout) break;
            }

            Console.WriteLine(imgResponse);
            return imgResponse.result.Value.sample;
        }

        private static async Task<Response> GetImageFromAPI(string id)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"{Config.GET_RESULT}{id}");
            var responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine("res: " + responseData);
            var res = JsonConvert.DeserializeObject<Response>(responseData);

            if (res.status == "Ready") return res;

            return null;
        }
    }
}
