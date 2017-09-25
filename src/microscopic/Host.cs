using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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

        public static IResponse PlainText(string value)
        {
            return new StringResponse(value, "text/plain");
        }

        public static IResponse Json(object value)
        {
            return new JsonResponse(value);
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

            if (req.HasBody)
            {
                try
                {
                    req.BodyStream.Dispose();
                }
                catch (Exception)
                {
                    // Already disposed
                }
            }

            using (var responseStream = await result.SerializeToStreamAsync())
            {
                context.Response.StatusCode = result.StatusCode;
                context.Response.ContentType = result.Headers.FirstOrDefault(x => x.Key == "Content-Type").Value ?? "text/html";
                context.Response.ContentLength64 = responseStream.Length;

                // Copy the response stream to the output stream
                await responseStream.CopyToAsync(context.Response.OutputStream);

                // Close the response to send it to the user
                context.Response.Close();
            }
        }

        public static Task StartAsync(string host, int port, CancellationTokenSource token, Func<Request, Task<IResponse>> handler)
        {
            return Task.Run(async () =>
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
                    // Was previously using `var task = await Task.WhenAny(queue)` to wait for tasks and get the next completed one,
                    // but that introduced an issue where a token could never break the loop via cancelling until a request was received
                    // (and the request task completed thus continuing the loop).
                    var task = queue.FirstOrDefault(t => t.IsCompleted);

                    if (task == null)
                    {
                        continue;
                    }

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
            });
        }

        public static Task StartAsync(string host, int port, CancellationTokenSource token, Func<Request, IResponse> handler)
        {
            return StartAsync(host, port, token, (req) => Task.Run<IResponse>(() => handler(req)));
        }

        public static Task StartAsync(string host, int port, CancellationTokenSource token, Func<Request, string> handler)
        {
            return StartAsync(host, port, token, (req) => Task.Run<IResponse>(() => new HtmlResponse(handler(req))));
        }

        public static Task StartAsync(string host, int port, CancellationTokenSource token, Func<Request, Task<string>> handler)
        {
            return StartAsync(host, port, token, async (req) =>
            {
                string response = await handler(req);

                return new HtmlResponse(response);
            });
        }

        public static Task StartAsync(string host, int port, CancellationTokenSource token, string html)
        {
            return StartAsync(host, port, token, (req) => new HtmlResponse(html));
        }
    }
}
