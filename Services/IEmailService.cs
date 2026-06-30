namespace ApnaKrishi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendOrderConfirmationAsync(string toEmail, string userName, int orderId, decimal amount);
    }
}
