using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
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

        public static async Task Start(string host, int port, CancellationTokenSource token, Func<Request, Task<IResponse>> handler)
        {
            var listener = new HttpListener();
            var address = BuildListenerPrefix(host, port);

            listener.Prefixes.Add(address);
            listener.Start();

            Console.WriteLine($"Microscopic: accepting connections at {address}");

            while (!token.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync();
                var contextReq = context.Request;
                var req = new Request(contextReq);
                IResponse result;

                try
                {
                    result = await handler(req);
                }
                catch (Exception e)
                {
                    result = Json(e);
                    result.StatusCode = 500;
                }

                var stringBytes = System.Text.Encoding.UTF8.GetBytes(result.SerializeToString());

                context.Response.StatusCode = result.StatusCode;
                context.Response.ContentType = result.Headers.FirstOrDefault(x => x.Key == "Content-Type").Value ?? "text/html";
                context.Response.ContentLength64 = stringBytes.Length;
                context.Response.OutputStream.Write(stringBytes, 0, stringBytes.Length);
                context.Response.Close();
            }

            listener.Stop();
        }

        public static Task Start(string host, int port, CancellationTokenSource token, Func<Request, IResponse> handler)
        {
            return Start(host, port, token, (req) => Task.Run<IResponse>(() => handler(req)));
        }

        public static Task Start(string host, int port, CancellationTokenSource token, string html)
        {
            return Start(host, port, token, (req) => Html(html));
        }
    }
}
