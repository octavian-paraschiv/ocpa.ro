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

    public enum UserType
    {
        ApiUser = 0,
        Admin = 1,
        Patient = 2
    }

    public class User
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string LoginId { get; set; }

        public string PasswordHash { get; set; }

        public UserType Type { get; set; }
    }

    public class AuthenticateResponse
    {
        [Required]
        public string LoginId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public UserType Type { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        [Required]
        public int Validity { get; set; }
    }

}
