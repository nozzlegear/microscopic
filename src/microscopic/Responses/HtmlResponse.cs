using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microscopic.Responses
{
    public class HtmlResponse : IResponse
    {
        private string Html { get; set; }

        public HtmlResponse(string html)
        {
            Html = html;

            Headers.Add("Content-Type", "text/html");
        }

        public int StatusCode { get; set; } = 200;

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public Task<Stream> SerializeToStreamAsync()
        {
            return Task.Run<Stream>(() =>
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(Html);

                return new MemoryStream(bytes);
            });
        }
    }
}