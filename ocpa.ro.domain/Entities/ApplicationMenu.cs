using ocpa.ro.domain.Abstractions;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities;

public partial class ApplicationMenu : IDbEntity
{
    public int Id { get; set; }

    public int? ApplicationId { get; set; }

    public int MenuId { get; set; }

    [JsonIgnore]
    public virtual Application Application { get; set; }

    [JsonIgnore]
    public virtual Menu Menu { get; set; }
}
