using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microscopic.Responses
{
    public class JsonResponse : IResponse
    {
        private Object Value { get; set; }

        public JsonResponse(object value)
        {
            Value = value;

            Headers.Add("Content-Type", "application/json");
        }

        public int StatusCode { get; set; } = 200;

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public Task<Stream> SerializeToStreamAsync()
        {
            return Task.Run<Stream>(() =>
            {
                var content = JsonConvert.SerializeObject(Value);
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);

                return new MemoryStream(bytes);
            });
        }
    }
}