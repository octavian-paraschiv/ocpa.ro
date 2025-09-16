using ocpa.ro.domain.Abstractions;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities;

public partial class City : IDbEntity
{
    public int Id { get; set; }

    public int RegionId { get; set; }

    public string Name { get; set; }

    public string Subregion { get; set; }

    public float Lat { get; set; }

    public float Lon { get; set; }

    public bool IsDefault { get; set; }

    [JsonIgnore]
    public virtual Region Region { get; set; }
}
