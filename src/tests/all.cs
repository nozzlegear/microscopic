using System;
using Xunit;
using Microscopic;
using Microscopic.Responses;
using System.Threading.Tasks;
using System.Threading;

namespace tests
{
    public class All
    {
        [Fact(DisplayName = "Returns Html")]
        public async Task Html()
        {
            var token = new CancellationTokenSource();
            token.CancelAfter(2000);

            await Host.Start("localhost", 8000, token, "<h1>Hello world! Microscopic can return HTML strings.</h1>");
        }

        [Fact(DisplayName = "Returns strings")]
        public async Task Strings()
        {
            int i = 0;
            var token = new CancellationTokenSource();
            var host = Host.Start("localhost", 8000, token, (req) =>
            {
                i++;
                return new StringResponse($"<hello>world ${i}</hello>", "application/xml");
            });

            while (true)
            {
                if (i >= 3)
                {
                    token.Cancel();
                    break;
                }
            }

            await host;
        }

        [Fact(DisplayName = "Returns JSON")]
        public async Task Json()
        {
            var token = new CancellationTokenSource();
            int i = 0;

            var host = Host.Start("localhost", 8000, token, (req) =>
            {
                i++;
                return Host.Json(new { hello = "world", foo = true });
            });

            while (true)
            {
                if (i >= 1)
                {
                    token.Cancel();
                    break;
                }
            }

            await host;
        }

        [Fact(DisplayName = "Handles Async")]
        public async Task HandlesAsync()
        {
            var token = new CancellationTokenSource();
            int i = 0;

            var host = Host.Start("localhost", 8000, token, async (req) =>
            {
                await Task.Delay(1000);

                i++;

                return Host.Html("<h1>Microscopic sent this string after waiting 1 seconds asynchronously.</h1>");
            });

            while (true)
            {
                if (i >= 1)
                {
                    token.Cancel();
                    break;
                }
            }

            await host;
        }

        [Fact(DisplayName = "Catches errors")]
        public async Task CatchesErrors()
        {
            var token = new CancellationTokenSource();
            bool thrown = false;

            var host = Host.Start("localhost", 8000, token, (req) =>
            {
                if (!thrown)
                {
                    thrown = true;

                    throw new Exception("Something crazy happened by Microscopic caught it and didn't take down the server!");
                }

                return Host.Html("Donezo");
            });

            while (!thrown) { }

            token.Cancel();

            await host;
        }
    }
}
