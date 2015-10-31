using System.IO;
using System.Text;
using Nancy;

namespace Hepsi.Http.Client.ApiTests
{
    public static class NancyResponseExtensions
    {
        public static string AsString(this Response response)
        {
            var memoryStream = new MemoryStream();
            response.Contents(memoryStream);

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
