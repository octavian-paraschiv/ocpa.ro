using ocpa.ro.domain.Abstractions.Database;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ocpa.ro.domain.Entities.Application;

public class UserType : IDbEntity
{
    public int Id { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }

    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
