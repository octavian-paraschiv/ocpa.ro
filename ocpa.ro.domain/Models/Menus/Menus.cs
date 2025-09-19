using ocpa.ro.domain.Entities.Application;
using System.Collections.Generic;

namespace ocpa.ro.domain.Models.Menus;
public class Menus
{
    public IEnumerable<VMenu> PublicMenus { get; set; }
    public IEnumerable<AppMenu> AppMenus { get; set; }
    public string DeviceId { get; set; }
}
