namespace Visage.FrontEnd.Shared.Services;

public interface ICloudinaryImageSigningService
{
    Task<CloudinaryUploadParams> SignUploadAsync();
}
