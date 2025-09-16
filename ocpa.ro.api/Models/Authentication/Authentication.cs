using System.ComponentModel.DataAnnotations;

namespace ocpa.ro.api.Models.Authentication
{
    public class AuthenticateRequest
    {
        [Required]
        public string LoginId { get; set; }

        [Required]
        public string Password { get; set; }
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
}
