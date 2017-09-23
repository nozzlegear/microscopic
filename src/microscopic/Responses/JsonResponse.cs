using System;
using System.Collections.Generic;
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

        public string SerializeToString()
        {
            return JsonConvert.SerializeObject(Value);
        }
    }
}