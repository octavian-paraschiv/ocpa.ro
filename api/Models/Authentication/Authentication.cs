using System;
using System.ComponentModel.DataAnnotations;
using ThorusCommon.SQLite;

namespace ocpa.ro.api.Models.Authentication
{
    public class AuthenticateRequest
    {
        [Required]
        public string LoginId { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class UserType
    {
        [PrimaryKey]
        public int Id { get; set; }

        [NotNull]
        public string Code { get; set; }

        [NotNull]
        public string Description { get; set; }
    }

    public class User
    {
        [PrimaryKey]
        public int Id { get; set; }

        [NotNull]
        public string LoginId { get; set; }

        [NotNull]
        public string PasswordHash { get; set; }

        [NotNull]
        public int Type { get; set; }
    }

    public class AuthenticateResponse
    {
        [Required]
        public string LoginId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public int Type { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        [Required]
        public int Validity { get; set; }
    }

    public class RegisteredDevice
    {
        [PrimaryKey]
        public int Id { get; set; }

        [NotNull]
        public string DeviceId { get; set; }

        [NotNull]
        public string LastLoginId { get; set; }

        [NotNull]
        public DateTime LastLoginTimestamp { get; set; }

        [NotNull]
        public string LastLoginIpAddress { get; set; }

        [NotNull]
        public string LastLoginGeoLocation { get; set; }
    }
}
