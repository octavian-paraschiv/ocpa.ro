using ocpa.ro.domain.Entities;
using System.Collections.Generic;

namespace ocpa.ro.api.Models.Menus;
public class Menus
{
    public IEnumerable<VMenu> PublicMenus { get; set; }
    public IEnumerable<AppMenu> AppMenus { get; set; }
    public string DeviceId { get; set; }
}
