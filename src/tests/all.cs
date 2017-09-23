using System;
using Xunit;
using Microscopic;
using Microscopic.Responses;
using System.Threading.Tasks;

namespace tests
{
    public class All
    {
        [Fact(DisplayName = "Returns Html")]
        public void Html()
        {
            Host.Start("localhost", 8000, "<h1>Hello world! Microscopic can return HTML strings.</h1>");
        }

        [Fact(DisplayName = "Returns strings")]
        public void Strings()
        {
            Host.Start("localhost", 8000, () =>
            {
                return new StringResponse("<hello>world</hello>", "application/xml");
            });
        }

        [Fact(DisplayName = "Returns JSON")]
        public void Json()
        {
            Host.Start("localhost", 8000, () =>
            {
                return Host.Json(new { hello = "world", foo = true });
            });
        }

        [Fact(DisplayName = "Handles Async")]
        public void HandlesAsync()
        {
            Host.Start("localhost", 8000, async () =>
            {
                await Task.Delay(2000);

                return Host.Html("<h1>Microscopic sent this string after waiting 2 seconds asynchronously.</h1>");
            });
        }
    }
}
