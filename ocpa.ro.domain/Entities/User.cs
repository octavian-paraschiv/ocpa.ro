using ocpa.ro.domain.Abstractions;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities;

public partial class User : IDbEntity
{
    public int Id { get; set; }

    public string LoginId { get; set; }

    public string PasswordHash { get; set; }

    public int Type { get; set; }

    public bool Enabled { get; set; }

    public int LoginAttemptsRemaining { get; set; }

    public string EmailAddress { get; set; }

    public bool UseOtp { get; set; }

    [JsonIgnore]
    public virtual ICollection<ApplicationUser> Applicationusers { get; set; } = new List<ApplicationUser>();

    [JsonIgnore]
    public virtual ICollection<OneTimePassword> OneTimePasswords { get; set; } = new List<OneTimePassword>();

    [JsonIgnore]
    public virtual UserType TypeNavigation { get; set; }
}
