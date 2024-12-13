using ThorusCommon.SQLite;

namespace ocpa.ro.api.Models.Applications
{
    public class Application
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string Code { get; set; }

        public bool LoginRequired { get; set; }

        public bool AdminMode { get; set; }

        public bool Builtin { get; set; }
    }

    public class ApplicationMenu
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int ApplicationId { get; set; }

        [NotNull]
        public int MenuId { get; set; }
    }

    public class ApplicationUser
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int ApplicationId { get; set; }

        [NotNull]
        public int UserId { get; set; }
    }
}
