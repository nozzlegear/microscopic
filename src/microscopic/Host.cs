using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microscopic.Responses;

namespace Microscopic
{
    public static class Host
    {
        static string BuildListenerPrefix(string host, int port)
        {
            var builder = new UriBuilder()
            {
                Host = host,
                Port = port,
                Scheme = "http"
            };

            return builder.ToString();
        }

        public static IResponse Json(object value)
        {
            return new JsonResponse(value);
        }

        public static IResponse File(string fileName)
        {
            return new StringResponse(System.IO.File.ReadAllText(fileName));
        }

        public static IResponse Html(string html)
        {
            return new HtmlResponse(html);
        }

        public static void Start(string host, int port, Func<IResponse> handler)
        {
            var listener = new HttpListener();
            var address = BuildListenerPrefix(host, port);

            listener.Prefixes.Add(address);
            listener.Start();

            Console.WriteLine($"Microscopic: accepting connections at {address}");

            var context = listener.GetContext();
            var result = handler();
            var stringBytes = System.Text.Encoding.UTF8.GetBytes(result.SerializeToString());

            context.Response.ContentType = result.Headers.FirstOrDefault(x => x.Key == "Content-Type").Value ?? "text/html";
            context.Response.ContentLength64 = stringBytes.Length;
            context.Response.OutputStream.Write(stringBytes, 0, stringBytes.Length);
            context.Response.Close();

            listener.Stop();
        }

        public static void Start(string host, int port, Func<Task<IResponse>> handler)
        {
            Start(host, port, () =>
            {
                var result = handler().ConfigureAwait(false).GetAwaiter().GetResult();

                return result;
            });
        }

        public static void Start(string host, int port, string html)
        {
            Start(host, port, () => Html(html));
        }
    }
}
