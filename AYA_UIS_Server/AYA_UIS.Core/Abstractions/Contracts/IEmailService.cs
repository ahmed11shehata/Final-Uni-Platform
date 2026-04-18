namespace AYA_UIS.Application.Contracts
{
    public class EmailResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public string Provider { get; set; } = "";

        public static EmailResult Success(string provider)
            => new() { Succeeded = true, Provider = provider };

        public static EmailResult Failure(string provider, string error)
            => new() { Succeeded = false, Provider = provider, ErrorMessage = error };
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task<EmailResult> SendEmailWithResultAsync(string to, string subject, string body);
    }
}
