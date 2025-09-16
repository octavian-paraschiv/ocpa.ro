using System.Threading.Tasks;

namespace ocpa.ro.domain.Abstractions.Gateways;

public interface IEmailGateway
{
    Task SendEmail(string[] recipients, string subject, string message);
}
