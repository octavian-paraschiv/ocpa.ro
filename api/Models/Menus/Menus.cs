﻿using ThorusCommon.SQLite;

namespace ocpa.ro.api.Models.Menus
{
    public class Menu
    {
        [PrimaryKey]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string Url { get; set; }

        [NotNull]
        public string Code { get; set; }
    }

#pragma warning disable S2094 // Classes should not be empty
    public class PublicMenu : Menu
    {
    }
#pragma warning restore S2094 // Classes should not be empty

    public class AppMenu : Menu
    {
        public int? UserId { get; set; }

        public string AppName { get; set; }
    }
}