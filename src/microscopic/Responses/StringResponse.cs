using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        public Task<Stream> SerializeToStreamAsync()
        {
            return Task.Run<Stream>(() =>
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(Value);

                return new MemoryStream(bytes);
            });
        }
    }
}