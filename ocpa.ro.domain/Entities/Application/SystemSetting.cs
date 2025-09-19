using ocpa.ro.domain.Abstractions.Database;

namespace ocpa.ro.domain.Entities.Application;

public class SystemSetting : IDbEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }

    public string Type { get; set; }
}
