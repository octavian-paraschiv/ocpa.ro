using ocpa.ro.domain.Abstractions.Database;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities.Application;

public class MenuDisplayMode : IDbEntity
{
    public int Id { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }

    [JsonIgnore]
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
}
