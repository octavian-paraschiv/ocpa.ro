using ocpa.ro.domain.Abstractions;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities;

public enum EMenuDisplayMode
{
    AlwaysHide = 0,
    AlwaysShow,
    HideOnMobile,
    ShowOnMobile,
};

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

public class VMenu : IDbEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Url { get; set; }

    public EMenuDisplayMode DisplayMode { get; set; }

    public string MenuIcon { get; set; }
}

#pragma warning disable S2094 // Classes should not be empty
public class PublicMenu : VMenu
{
}
#pragma warning restore S2094 // Classes should not be empty

public class AppMenu : VMenu
{
    public int? UserId { get; set; }

    public string AppName { get; set; }

    public bool AdminMode { get; set; }
}