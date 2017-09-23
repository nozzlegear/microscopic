using System.Collections.Generic;

namespace Microscopic.Responses
{
    public class StringResponse : IResponse
    {
        private string Value { get; set; }

        public StringResponse(string value)
        {
            Value = value;
        }

        public StringResponse(string value, string contentType)
        {
            Value = value;

            Headers.Add("Content-Type", contentType);
        }

        public int StatusCode { get; set; } = 200;

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public string SerializeToString()
        {
            return Value;
        }
    }
}