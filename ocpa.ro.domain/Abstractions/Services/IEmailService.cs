using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocpa.ro.domain.Abstractions.Services;

public interface IEmailService
{
    Task SendOneTimePassword(string recipient, string mfa, string language);
}