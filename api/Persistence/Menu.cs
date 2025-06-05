using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ocpa.ro.api.Persistence;

public partial class Menu : IDbEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Url { get; set; }

    public int? DisplayModeId { get; set; }

    public string MenuIcon { get; set; }

    public bool Builtin { get; set; }

    [JsonIgnore]
    public virtual ICollection<ApplicationMenu> ApplicationMenus { get; set; } = new List<ApplicationMenu>();

    [JsonIgnore]
    public virtual MenuDisplayMode DisplayMode { get; set; }
}
