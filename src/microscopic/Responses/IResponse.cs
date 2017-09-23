using System.Collections.Generic;

namespace Microscopic.Responses
{
    public interface IResponse
    {
        Dictionary<string, string> Headers { get; }

        int StatusCode { get; set; }

        string SerializeToString();
    }
}