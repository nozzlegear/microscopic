# microscopic

A tiny .NET package for serving HTTP microservices. Based off of Zeit's micro for Node.

# Features

- **Easy**: Designed for usage with async and await.
- **Micro**: The whole project is ~260 lines of code.
- **Agile**: Super easy deployment and containerization.
- **Simple**: Designed for single-purpose HTTP functions.
- **Standard**: Uses System.Net.HttpListener to create a tiny HTTP listener.
- **Explicit**: No middleware or frameworks getting in the way.

# Usage

Install Microscopic from Nuget:

```sh
dotnet add package microscopic
# Or with Paket:
# paket add microscopic
```

A Microscopic listener will run indefinitely, so it's important to create a `CancellationTokenSource` which will let you stop the listener when necessary. Once you've got a cancellation token ready you can start Microscopic. The listener runs asynchronously off the main thread so as not to block the rest of your program, and you can await it which will wait for the task to complete.

Just keep in mind the task never completes until your cancellation token cancels it.

```cs
static async void Main(string args[])
{
    var token = new CancellationTokenSource();
    var listener = Microscopic.Listener.StartAsync("localhost", 8000, token, (req) => "<h1>Hello world! This was sent from Microscopic at http://localhost:8000</h1>");

    // The listener.

    // Cancel the listener after 30 seconds
    token.CancelAfter(30000)

    // Wait for the listener task to complete. Remember, it only completes after the token cancels!
    await listener;

    // Microscopic listener is no longer running!
}
```

# Return Types

Microscopic wants you to return either a string which will be encoded as HTML, or an instance of `Microscopic.Responses.IResponse` which lets you control the response's headers, status code and content. You can also return `Task<string>` and `Task<Microscopic.Responses.IResponse>` for async functions.

```cs
var listener = Microscopic.Listener.StartAsync("localhost", 8000, token, async (req) =>
{
    // Do some async work here.
    await Task.Delay(2000);

    // Return a string, which will be encoded as HTML.
    return "<h1>Hello world! This was sent from an async Microscopic function.</h1>";
});
```

Microscopic's Listener provides several helper functions for returning JSON (serialized with `Newtonsoft.Json`), Plain Text and Empty responses.

```cs
// Return JSON with Listener.Json
var listener = Microscopic.Listener.StartAsync("localhost", 8000, token, (req) => Listener.Json(new { foo = "hello world!", bar = false, baz = 117 }))
```

```cs
// Return Plain Text with Listener.PlainText
var listener = Microscopic.Listener.StartAsync("localhost", 8000, token, (req) => Listener.PlainText("Hello world! This response's content type will be text/plain."));
```

```cs
// Return an empty response with Listener.Empty
var listener = Microscopic.Listener.StartAsync("localhost", 8000, token, (req) => Listener.Empty());
```

If those helper functions aren't enough, you can also return your own custom `IResponse`:

```cs
public class MyImageResponse : Microscopic.Responses.IResponse
{
    private MyImage Img { get; set; }

    public PdfResponse(MyImage img)
    {
        Img = img;

        Headers.Add("Content-Type", "image/png");
    }

    public int StatusCode { get; set; } = 200;

    public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

    public async Task<Stream> SerializeToStreamAsync()
    {
        var imageBytes = await Img.DoSomethingToConvertToBytes();

        return new MemoryStream(bytes);
    }
}
```

```cs
var listener = Microscopic.Listener.StartAsync("localhost", 8000, token, (req) => new MyImageResponse(new MyImage("path/to/image.png")));
```

# Errors

Microscopic will wrap your handler in a try/catch block. If it throws an error we'll serialize the Exception and return it with a `500` status code for easier debugging. This function:

```cs
var listener = Microscopic.Listener.StartAsync("localhost", 8000, token, (req) => throw new Exception("Something happened!"));
```

Would return this JSON with a `500 Internal Server Error` status:

```json
{
    "ClassName": "System.Exception",
    "Message": "Something happened!",
    "Data": null,
    "InnerException": null,
    "HelpURL": null,
    "StackTraceString": "   at tests.All.<>c__DisplayClass15_0.<ErrorTest>b__0(Request req) in c:\\users\\nozzlegear\\source\\microscopic\\src\\tests\\all.cs:line 452\r\n   at Microscopic.Listener.<>c__DisplayClass6_1.<StartAsync>b__1() in c:\\users\\nozzlegear\\source\\microscopic\\src\\microscopic\\Host.cs:line 135\r\n   at System.Threading.Tasks.Task`1.InnerInvoke()\r\n   at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)\r\n   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot)\r\n--- End of stack trace from previous location where exception was thrown ---\r\n   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()\r\n   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)\r\n   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()\r\n   at Microscopic.Listener.<ProcessRequestAsync>d__4.MoveNext() in c:\\users\\nozzlegear\\source\\microscopic\\src\\microscopic\\Host.cs:line 51",
    "RemoteStackTraceString": null,
    "RemoteStackIndex": 0,
    "ExceptionMethod": null,
    "HResult": -2146233088,
    "Source": "tests",
    "WatsonBuckets": null
}
```