using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        public string FunctionHandler(Stream inputStream, ILambdaContext context)
        {
            using var reader = new StreamReader(inputStream);
            using var jsonReader = new JsonTextReader(reader);
            var json = JObject.Load(jsonReader);

            // Assuming the structure of your input JSON matches what you expect from GitHub webhook or your test.
            var issueUrl = json["issue"]?["html_url"]?.ToString();

            if (string.IsNullOrEmpty(issueUrl))
            {
                return "No issue URL found.";
            }

            string payload = $"{{\"text\":\"Issue Created: {issueUrl}\"}}";

            var client = new HttpClient();
            var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            var response = client.SendAsync(webRequest).Result;
            using var responseReader = new StreamReader(response.Content.ReadAsStreamAsync().Result);
                
            return responseReader.ReadToEnd();
        }
    }
}
