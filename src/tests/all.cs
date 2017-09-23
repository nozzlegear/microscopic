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
        public void Html()
        {
            var token = new CancellationTokenSource();
            token.CancelAfter(2000);

            Host.Start("localhost", 8000, token, "<h1>Hello world! Microscopic can return HTML strings.</h1>");
        }

        [Fact(DisplayName = "Returns strings")]
        public void Strings()
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
        }

        [Fact(DisplayName = "Returns JSON")]
        public void Json()
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
        }

        [Fact(DisplayName = "Handles Async")]
        public void HandlesAsync()
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
        }

        [Fact(DisplayName = "Catches errors")]
        public void CatchesErrors()
        {
            // var token = new CancellationTokenSource();
            // token.CancelAfter(3000);

            // Host.Start("localhost", 8000, token, (req) =>
            // {
            //     Assert.True(false);
            //     return Host.Html("<h1 style='color: red'>Xunit should have hit an assertion failure.</h1>");
            // });
        }
    }
}
