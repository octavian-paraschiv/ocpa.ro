using ocpa.ro.domain.Abstractions.Database;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities.Application;

public class OneTimePassword : IDbEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Hash { get; set; }

    public string Expiration { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }
}
