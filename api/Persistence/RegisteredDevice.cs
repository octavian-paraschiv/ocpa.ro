using System;

namespace ocpa.ro.api.Persistence;

public partial class RegisteredDevice : IDbEntity
{
    public int Id { get; set; }

    public string DeviceId { get; set; }

    public string LastLoginId { get; set; }

    public string LastLoginIpAddress { get; set; }

    public DateTime LastLoginTimestamp { get; set; }

    public string LastLoginGeoLocation { get; set; }
}
