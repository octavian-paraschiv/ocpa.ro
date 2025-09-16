using ocpa.ro.domain.Abstractions;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities;



public partial class Application : IDbEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public bool LoginRequired { get; set; }

    public bool AdminMode { get; set; }

    public bool Builtin { get; set; }

    [JsonIgnore]
    public virtual ICollection<ApplicationMenu> ApplicationMenus { get; set; } = new List<ApplicationMenu>();

    [JsonIgnore]
    public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; } = new List<ApplicationUser>();
}
