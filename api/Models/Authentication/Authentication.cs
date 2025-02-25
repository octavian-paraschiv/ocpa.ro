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
        [AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Code { get; set; }

        [NotNull]
        public string Description { get; set; }
    }

    public class User
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string LoginId { get; set; }

        [NotNull]
        public string PasswordHash { get; set; }

        [NotNull]
        public int Type { get; set; }

        [NotNull]
        public bool Enabled { get; set; }

        [NotNull]
        public int LoginAttemptsRemaining { get; set; }

        [NotNull]
        public string EmailAddress { get; set; }

        [NotNull]
        public bool UseOTP { get; set; }
    }

    public class OneTimePassword
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int UserId { get; set; }

        [NotNull]
        public string Hash { get; set; }

        [NotNull]
        public DateTime Expiration { get; set; }
    }

    public class AuthenticationResponse
    {
        [Required]
        public string LoginId { get; set; }

        [Required]
        public string AnonymizedEmail { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public int Type { get; set; }

        [Required]
        public int Validity { get; set; }

        [Required]
        public bool SendOTP { get; set; }
    }

    public class FailedAuthenticationResponse
    {
        public string ErrorMessage { get; private set; } = string.Empty;
        public int LoginAttemptsRemaining { get; private set; } = -1;

        public FailedAuthenticationResponse(string errorMessage, int loginAttemptsRemaining = -1)
        {
            ErrorMessage = errorMessage;
            LoginAttemptsRemaining = loginAttemptsRemaining;
        }
    }

    public class RegisteredDevice
    {
        [PrimaryKey]
        [AutoIncrement]
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
