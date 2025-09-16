using ocpa.ro.domain.Abstractions;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities;

public partial class ApplicationUser : IDbEntity
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int UserId { get; set; }

    [JsonIgnore]
    public virtual Application Application { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }
}
