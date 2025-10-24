namespace Visage.FrontEnd.Shared.Services
{
    public sealed class ImageCdnOptions
    {
        public bool Enabled { get; set; } = true;
        public string UploadSegment { get; set; } = "/upload/";
        public string TransformSegment { get; set; } = "/upload/f_auto,q_auto,w_800/";
    }
}
