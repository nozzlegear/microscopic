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

        public static IResponse Empty()
        {
            return new StringResponse("");
        }

        private static async Task ProcessRequestAsync(HttpListenerContext context, Func<Request, Task<IResponse>> handler)
        {
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

        public static async Task Start(string host, int port, CancellationTokenSource token, Func<Request, Task<IResponse>> handler)
        {
            var listener = new HttpListener();
            var address = BuildListenerPrefix(host, port);

            listener.Prefixes.Add(address);
            listener.Start();

            Console.WriteLine($"Microscopic: accepting connections at {address}");

            var queue = new HashSet<Task>();

            for (int i = 0; i < 100; i++)
            {
                queue.Add(listener.GetContextAsync());
            }

            while (!token.IsCancellationRequested)
            {
                token.Token.ThrowIfCancellationRequested();

                var task = await Task.WhenAny(queue);

                // Remove the task from the queue. It maybe a listener with a request, or a completed handler.
                queue.Remove(task);

                if (task is Task<HttpListenerContext>)
                {
                    var context = await (task as Task<HttpListenerContext>);

                    // Add the handler to the async queue, then add a new listener to get us back to the desired number of listeners.
                    queue.Add(ProcessRequestAsync(context, handler));
                    queue.Add(listener.GetContextAsync());
                }
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
