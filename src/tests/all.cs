using System;
using Xunit;
using Microscopic;
using Microscopic.Responses;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Xunit.Sdk;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using System.Net.Http.Headers;
using System.Linq;
using System.IO;

namespace tests
{
    public class TestClass
    {
        public string Foo { get; set; }

        public bool Bar { get; set; }

        public int Baz { get; set; }
    }

    public class All
    {
        private string GetHeaderValue(HttpResponseHeaders headers, string key)
        {
            if (!headers.Contains(key))
            {
                return null;
            }

            var values = headers.GetValues(key);

            return values.First();
        }

        private string GetHeaderValue(HttpContentHeaders headers, string key)
        {
            if (!headers.Contains(key))
            {
                return null;
            }

            var values = headers.GetValues(key);

            return values.First();
        }

        private async Task<HttpResponseMessage> SendRequest()
        {
            return await SendRequest("http://localhost:8000", HttpMethod.Get);
        }

        private async Task<HttpResponseMessage> SendRequest(string url, HttpMethod method)
        {
            var headers = new Dictionary<string, string>();

            return await SendRequest(url, method, headers);
        }

        private async Task<HttpResponseMessage> SendRequest(string url, HttpMethod method, Dictionary<string, string> headers, object body = null)
        {
            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage(method, url);

                if (body != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                }

                foreach (var header in headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }

                return await client.SendAsync(message);
            }
        }

        [Fact(DisplayName = "Returns Html")]
        public async Task Html()
        {
            string expected = "<h1>Hello world! Microscopic can return HTML strings.</h1>";
            var token = new CancellationTokenSource();

            try
            {
                var host = Listener.StartAsync("localhost", 8000, token, expected);
                var response = await SendRequest();
                var responseBody = await response.Content.ReadAsStringAsync();

                token.Cancel();
                await host;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("text/html", GetHeaderValue(response.Content.Headers, "Content-Type"));
                Assert.Equal(expected, responseBody);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }

        }

        [Fact(DisplayName = "Returns strings")]
        public async Task Strings()
        {
            string expected = "<hello>world</hello>";
            var token = new CancellationTokenSource();

            try
            {
                var host = Listener.StartAsync("localhost", 8000, token, (req) => new StringResponse(expected, "application/xml"));
                var response = await SendRequest();
                var responseBody = await response.Content.ReadAsStringAsync();

                token.Cancel();
                await host;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("application/xml", GetHeaderValue(response.Content.Headers, "Content-Type"));
                Assert.Equal(expected, responseBody);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }
        }

        [Fact(DisplayName = "Returns JSON")]
        public async Task Json()
        {
            var data = new { hello = "world", foo = true };
            string expected = JsonConvert.SerializeObject(data);
            var token = new CancellationTokenSource();

            try
            {
                var host = Listener.StartAsync("localhost", 8000, token, (req) => Listener.Json(data));
                var response = await SendRequest();
                var responseBody = await response.Content.ReadAsStringAsync();

                token.Cancel();
                await host;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("application/json", GetHeaderValue(response.Content.Headers, "Content-Type"));
                Assert.Equal(expected, responseBody);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }
        }

        [Fact(DisplayName = "Handles Async")]
        public async Task HandlesAsync()
        {
            string expected = "<h1>Microscopic sent this string after waiting 1 seconds asynchronously.</h1>";
            var token = new CancellationTokenSource();

            try
            {
                var host = Listener.StartAsync("localhost", 8000, token, async (req) =>
                {
                    await Task.Delay(1000);

                    return expected;
                });
                var response = await SendRequest();
                var responseBody = await response.Content.ReadAsStringAsync();

                token.Cancel();
                await host;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("text/html", GetHeaderValue(response.Content.Headers, "Content-Type"));
                Assert.Equal(expected, responseBody);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }
        }

        [Fact(DisplayName = "Catches errors")]
        public async Task CatchesErrors()
        {
            string expected = "My random exception message.";
            var token = new CancellationTokenSource();
            bool shouldThrow = true;

            try
            {
                var host = Listener.StartAsync("localhost", 8000, token, (req) =>
                {
                    if (shouldThrow)
                    {
                        throw new Exception(expected);
                    }

                    return expected;
                });
                var response = await SendRequest();
                var responseBody = await response.Content.ReadAsStringAsync();

                token.Cancel();
                await host;

                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal("application/json", GetHeaderValue(response.Content.Headers, "Content-Type"));
                Assert.Contains(expected, responseBody);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }
        }

        [Fact(DisplayName = "Has all expected properties")]
        public async Task HasProperties()
        {
            Exception exception = null;
            var token = new CancellationTokenSource();

            try
            {
                var host = Listener.StartAsync("localhost", 8000, token, (req) =>
                {
                    try
                    {
                        Assert.NotNull(req.AcceptTypes);
                        Assert.NotNull(req.Cookies);
                        Assert.NotNull(req.Headers);
                        Assert.True(req.IsLocal);
                        Assert.Equal(HttpMethod.Get, req.Method);
                        Assert.NotNull(req.QueryString);
                        Assert.NotEmpty(req.RawUrl);
                        // Assert.NotNull(req.Referrer);
                        Assert.NotEmpty(req.RequestId);
                        Assert.NotNull(req.Url);
                        // Assert.NotNull(req.UserAgent);
                        Assert.NotEmpty(req.UserHostAddress);
                        Assert.NotEmpty(req.UserHostName);
                    }
                    catch (XunitException ex)
                    {
                        // Wrap the original exception to preserve its stack trace. If you don't do this the stack trace will say this catch block threw the exception rather than the assertion.
                        exception = new Exception(ex.Message, ex);

                        throw exception;
                    }

                    return Listener.Empty();
                });
                var response = await SendRequest();
                var responseBody = await response.Content.ReadAsStringAsync();

                token.Cancel();
                await host;

                if (exception != null)
                {
                    throw exception;
                }

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }
        }

        [Fact(DisplayName = "Does not drop requests.")]
        public async Task DoesNotDropRequests()
        {
            string expected = "<h1>Hello world! This test tries to ensure that Microscopic does not drop request queues.</h1>";
            var random = new Random();
            var token = new CancellationTokenSource();

            try
            {
                var host = Listener.StartAsync("localhost", 8000, token, async (req) =>
                {
                    var delayLength = random.Next(0, 3) * 1000;

                    await Task.Delay(delayLength);

                    return expected;
                });

                await Task.WhenAll(Enumerable.Range(0, 100).Select(async (i) =>
                {
                    var response = await SendRequest();
                    var responseBody = await response.Content.ReadAsStringAsync();

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("text/html", GetHeaderValue(response.Content.Headers, "Content-Type"));
                    Assert.Equal(expected, responseBody);
                }));

                token.Cancel();
                await host;
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }
        }

        [Fact(DisplayName = "Token stops server without any requests")]
        public async Task StopsWithoutRequests()
        {
            // Because of the way the server queue handlers are implemented (while loop that waits on listener tasks), we need to make sure
            // that a token can stop the server without first completing a task loop.
            var token = new CancellationTokenSource();
            token.CancelAfter(2000);

            await Listener.StartAsync("localhost", 8000, token, "Hello world!");

            Assert.True(true);
        }

        [Fact(DisplayName = "Request has BodyStream")]
        public async Task HasBodyStream()
        {
            string expected = "Assertions passed.";
            var data = new TestClass()
            {
                Foo = "Hello world!",
                Bar = true,
                Baz = 117
            };
            var token = new CancellationTokenSource();

            try
            {
                var host = Listener.StartAsync("localhost", 8000, token, async (req) =>
                {
                    Assert.True(req.HasBody);
                    Assert.NotNull(req.BodyStream);

                    TestClass body;

                    using (var reader = new StreamReader(req.BodyStream))
                    {
                        var json = await reader.ReadToEndAsync();
                        body = JsonConvert.DeserializeObject<TestClass>(json);
                    }

                    Assert.Equal(data.Foo, body.Foo);
                    Assert.Equal(data.Bar, body.Bar);
                    Assert.Equal(data.Baz, body.Baz);

                    return expected;
                });

                var result = await SendRequest("http://localhost:8000", HttpMethod.Get, new Dictionary<string, string>(), data);
                var output = await result.Content.ReadAsStringAsync();

                token.Cancel();
                await host;

                Assert.Equal(expected, output);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }
        }

        [Fact(DisplayName = "Receives headers")]
        public async Task ReceivesHeaders()
        {
            string headerName = "x-random-header";
            string headerValue = "Hello world! This is a header.";
            string expected = "Assertions passed.";
            var token = new CancellationTokenSource();

            try
            {
                var host = Listener.StartAsync("localhost", 8000, token, (req) =>
                {
                    Assert.True(req.Headers.AllKeys.Any(k => k == headerName));
                    Assert.Equal(headerValue, req.Headers.Get(headerName));

                    return expected;
                });

                var request = await SendRequest("http://localhost:8000", HttpMethod.Get, new Dictionary<string, string> { { headerName, headerValue } });
                var output = await request.Content.ReadAsStringAsync();

                token.Cancel();
                await host;

                Assert.Equal(expected, output);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    token.Cancel();
                }
            }
        }
    }
}
