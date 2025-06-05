using ocpa.ro.api.Persistence;
using System.Collections.Generic;

namespace ocpa.ro.api.Models.Menus
{
    public enum EMenuDisplayMode
    {
        AlwaysHide = 0,
        AlwaysShow,
        HideOnMobile,
        ShowOnMobile,
    };

    public class Menus
    {
        public IEnumerable<VMenu> PublicMenus { get; set; }
        public IEnumerable<AppMenu> AppMenus { get; set; }
        public string DeviceId { get; set; }
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
}
