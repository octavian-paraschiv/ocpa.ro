﻿using System.ComponentModel.DataAnnotations;

namespace ocpa.ro.api.Models
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
    }

}
