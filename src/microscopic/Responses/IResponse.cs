using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microscopic.Responses
{
    public interface IResponse
    {
        Dictionary<string, string> Headers { get; }

        int StatusCode { get; set; }

        Task<Stream> SerializeToStreamAsync();
    }
}