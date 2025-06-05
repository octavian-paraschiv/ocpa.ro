using System.Text.Json.Serialization;

namespace ocpa.ro.api.Persistence;

public partial class OneTimePassword : IDbEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Hash { get; set; }

    public string Expiration { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }
}
