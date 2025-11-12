using Microsoft.Extensions.Options;

namespace Visage.FrontEnd.Shared.Services
{
    public sealed class ConfigImageUrlTransformer(IOptions<ImageCdnOptions> options) : IImageUrlTransformer
    {
        private readonly ImageCdnOptions _opts = options.Value;

        public string Transform(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            if (!_opts.Enabled) return url;

            var upload = _opts.UploadSegment;
            var transform = _opts.TransformSegment;
            if (string.IsNullOrWhiteSpace(upload) || string.IsNullOrWhiteSpace(transform))
            {
                return url;
            }

            return url.Replace(upload, transform, StringComparison.OrdinalIgnoreCase);
        }
    }
}
