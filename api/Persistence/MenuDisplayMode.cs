using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ocpa.ro.api.Persistence;

public partial class MenuDisplayMode : IDbEntity
{
    public int Id { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }

    [JsonIgnore]
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
}
