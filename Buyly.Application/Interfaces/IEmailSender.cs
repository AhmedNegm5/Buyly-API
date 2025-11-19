using System.Threading.Tasks;

namespace Buyly.Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}

