using Refit;
using System.Net.Http;
using System.Threading.Tasks;


namespace Visage.FrontEnd.Shared.Services;


public interface ICloudinaryApi
{
    [Multipart]
    [Post("/v1_1/{cloudName}/image/upload")]
    Task<HttpResponseMessage> UploadImage(
        [AliasAs("cloudName")] string cloudName,
        [AliasAs("api_key")] string apiKey,
        [AliasAs("signature")] string signature,
        [AliasAs("timestamp")] string timestamp,
        [AliasAs("file")] StreamPart file);
}
