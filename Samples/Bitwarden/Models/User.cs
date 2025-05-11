using Bitwarden.Models.Enums;

namespace Bitwarden.Models;

public record User
{
    public string Email { get; set; }
    public string Password { get; set; }
    public Host Host { get; set; }
    public string AvatarIconInitials { get; set; }

    public User(
        string email,
        string password,
        string avatarIconInitials,
        Host host = BitwardenConstants.DefaultHost)
    {
        Email = email;
        Password = password;
        Host = host;
        AvatarIconInitials = avatarIconInitials;
    }
}