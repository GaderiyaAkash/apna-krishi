using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ApnaKrishi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:SenderName"] ?? "Apna Krishi",
                _config["Email:SenderEmail"] ?? "noreply@apnakrishi.com"));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Email:SmtpHost"] ?? "smtp.gmail.com",
                int.Parse(_config["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"]);

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendOrderConfirmationAsync(string toEmail, string userName, int orderId, decimal amount)
        {
            var html = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden'>
  <div style='background:#2e7d32;padding:20px;text-align:center'>
    <h1 style='color:white;margin:0'>🌾 Apna Krishi</h1>
  </div>
  <div style='padding:30px'>
    <h2 style='color:#2e7d32'>Order Confirmed!</h2>
    <p>Dear <strong>{userName}</strong>,</p>
    <p>Your order has been placed successfully.</p>
    <table style='width:100%;border-collapse:collapse;margin:20px 0'>
      <tr style='background:#f5f5f5'>
        <td style='padding:10px;font-weight:bold'>Order ID</td>
        <td style='padding:10px'>#{orderId}</td>
      </tr>
      <tr>
        <td style='padding:10px;font-weight:bold'>Total Amount</td>
        <td style='padding:10px'>₹{amount:F2}</td>
      </tr>
    </table>
    <p>We will notify you when your order is dispatched.</p>
    <p style='color:#666;font-size:12px'>Thank you for choosing Apna Krishi!</p>
  </div>
</div>";

            await SendEmailAsync(toEmail, $"Order Confirmation – Order #{orderId}", html);
        }
    }
}
