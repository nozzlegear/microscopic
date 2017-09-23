using System.Collections.Generic;

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

        public string SerializeToString()
        {
            return Html;
        }
    }
}