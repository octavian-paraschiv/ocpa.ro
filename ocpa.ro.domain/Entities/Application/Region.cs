using ocpa.ro.domain.Abstractions.Database;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities.Application;

public class Region : IDbEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public float MinLon { get; set; }

    public float MaxLon { get; set; }

    public float MinLat { get; set; }

    public float MaxLat { get; set; }

    public float GridResolution { get; set; }

    [JsonIgnore]
    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
