namespace Visage.Shared.Models;

public class UserProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LinkedIn { get; set; } = string.Empty;
    public string GitHub { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }

    public UserProfileDto Clone()
    {
        return (UserProfileDto)MemberwiseClone();
    }
}
