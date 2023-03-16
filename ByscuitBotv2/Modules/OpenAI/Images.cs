using byscuitBot;
using ByscuitBotv2.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Modules.OpenAI
{
    public class Images
    {
        private static string API_KEY = Config.API_KEY;
        private static string API_IMG_GEN = Config.API_IMG_GEN;
        private static string API_IMG_EDIT = Config.API_IMG_EDIT;

        /// <summary>
        /// Struct for image generation response
        /// </summary>
        struct Response
        {
            public uint created;
            public ResponseData[] data;
            public ResponseError? error;
        }
        struct ResponseData
        {
            public string url;
        }
        struct ResponseError
        {
            public string code;
            public string message;
            public string param;
            public string type;
        }
        // sizes 256x256, 512x512, or 1024x1024
        public static async Task<string> createImage(string prompt, string numberOfImgs, string size)
        {
            if (String.IsNullOrEmpty(prompt) ||
                String.IsNullOrEmpty(numberOfImgs) ||
                String.IsNullOrEmpty(size))
            {
                Utility.printERROR("Open.AI Image Generation missing parameter");
                throw new Exception("Missing Parameter");
            }
            string escapedInput = prompt.Replace("\"", "\\\"");
            string json = $"{{\"prompt\":\"{escapedInput}\"," +
                          $"\"n\":{numberOfImgs}," +
                          $"\"size\":\"{size}\"}}";
            Utility.printConsole("json: " + json);
            var jsonContent = new StringContent(json);
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
            var httpResponse = await httpClient.PostAsync(API_IMG_GEN, jsonContent);
            string result = await httpResponse.Content.ReadAsStringAsync();
            Utility.printConsole("result: " + result);
            Response response = JsonConvert.DeserializeObject<Response>(result);
            if (response.error != null) throw new Exception(response.error.Value.message);
            Utility.printConsole($"Open AI response: ${response.data[0].url}");
            return response.data[0].url;
        }


        public static async Task<string> editImage(string prompt, string numberOfImgs, string size, string image, string mask = null)
        {
            if (String.IsNullOrEmpty(prompt) ||
                String.IsNullOrEmpty(numberOfImgs) ||
                String.IsNullOrEmpty(size) ||
                String.IsNullOrEmpty(image))
            {
                Utility.printERROR("Open.AI Image Editor missing parameter");
                throw new Exception("Missing Parameter");
            }

            // Download images
            string tempPath = Directory.GetCurrentDirectory() + $"/User-Images/";
            string imageName = Path.GetFileName(image.Replace("https://",""));
            string fullPath = tempPath + imageName;
            string maskName = "";
            string maskFullPath = "";
            byte[] imageBytes = null;
            byte[] maskBytes = null;

            using (WebClient client = new WebClient())
            {
                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                Utility.printConsole("Fullpath to Image: " + fullPath);
                client.DownloadFile(new Uri(image), fullPath);
                imageBytes = File.ReadAllBytes(fullPath);

                if (mask != null)
                {
                    maskName = Path.GetFileName(mask.Replace("https://", ""));
                    maskFullPath = tempPath + maskName;
                    Utility.printConsole("Fullpath to mask: " + maskFullPath);
                    client.DownloadFile(new Uri(mask), maskFullPath);
                    maskBytes = File.ReadAllBytes(fullPath);
                }
            }
            

            var imgByteArray = new ByteArrayContent(imageBytes);
            var maskByteArray = mask != null ? new ByteArrayContent(maskBytes) : null;
            var content = new MultipartFormDataContent();

            content.Add(imgByteArray, "image", imageName);
            if (mask != null) content.Add(maskByteArray, "mask", maskName);
            string escapedInput = prompt.Replace("\"", "\\\"");
            content.Add(new StringContent(escapedInput), "prompt");
            content.Add(new StringContent(numberOfImgs), "n");
            content.Add(new StringContent(size), "size");

            Utility.printConsole("json: " + JsonConvert.SerializeObject(content));

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
            var httpResponse = await httpClient.PostAsync(API_IMG_EDIT, content);
            string result = await httpResponse.Content.ReadAsStringAsync();
            Utility.printConsole("result: " + result);
            Response response = JsonConvert.DeserializeObject<Response>(result);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            if (File.Exists(maskFullPath)) File.Delete(maskFullPath);
            if (response.error != null) throw new Exception(response.error.Value.message);
            Utility.printConsole($"Open AI response: ${response.data[0].url}");
            return response.data[0].url;
        }
    }
}
