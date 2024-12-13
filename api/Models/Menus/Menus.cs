using System.Collections.Generic;
using ThorusCommon.SQLite;

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

    public class VMenu
    {
        [PrimaryKey]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string Url { get; set; }

        [NotNull]
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
    }

    public class Menu
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string Url { get; set; }

        [NotNull]
        public int DisplayModeId { get; set; }

        public string MenuIcon { get; set; }

        public bool Builtin { get; set; }
    }

    public class MenuDisplayMode
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Code { get; set; }

        [NotNull]
        public string Description { get; set; }
    }
}
