using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AYA_UIS.Application.Contracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Legacy method — throws on failure (backward compat).
        /// </summary>
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var result = await SendEmailWithResultAsync(to, subject, body);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"[{result.Provider}] Email delivery failed: {result.ErrorMessage}");
        }

        /// <summary>
        /// Provider-based send with structured result.
        /// </summary>
        public async Task<EmailResult> SendEmailWithResultAsync(string to, string subject, string body)
        {
            var provider = _configuration["emailSettings:Provider"]?.ToLowerInvariant() ?? "smtp";

            _logger.LogInformation(
                "Sending email via [{Provider}] to {Recipient}, subject: {Subject}",
                provider, MaskEmail(to), subject);

            try
            {
                var result = provider switch
                {
                    "brevo"  => await SendViaBrevoAsync(to, subject, body),
                    "resend" => await SendViaResendAsync(to, subject, body),
                    _        => await SendViaSmtpAsync(to, subject, body),
                };

                if (result.Succeeded)
                    _logger.LogInformation(
                        "Email delivered via [{Provider}] to {Recipient}",
                        result.Provider, MaskEmail(to));
                else
                    _logger.LogError(
                        "Email delivery FAILED via [{Provider}] to {Recipient}: {Error}",
                        result.Provider, MaskEmail(to), result.ErrorMessage);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Email delivery EXCEPTION via [{Provider}] to {Recipient}: {Error}",
                    provider, MaskEmail(to), ex.Message);

                return EmailResult.Failure(provider, ex.Message);
            }
        }

        // ──────────────────────────────────────────────────────────────
        // SMTP Provider (MailKit) — works with Gmail, Outlook, any SMTP
        // ──────────────────────────────────────────────────────────────
        private async Task<EmailResult> SendViaSmtpAsync(string to, string subject, string body)
        {
            var smtpServer  = _configuration["emailSettings:SmtpServer"]!;
            var smtpPort    = int.Parse(_configuration["emailSettings:SmtpPort"]!);
            var username    = _configuration["emailSettings:Username"]!;
            var password    = _configuration["emailSettings:Password"]!;
            var senderName  = _configuration["emailSettings:SenderName"]!;
            var senderEmail = _configuration["emailSettings:SenderEmail"]!;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            client.Timeout = 15000; // 15s timeout

            await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return EmailResult.Success("smtp");
        }

        // ──────────────────────────────────────────────────────────────
        // Resend Provider (HTTP API)
        // ──────────────────────────────────────────────────────────────
        private async Task<EmailResult> SendViaResendAsync(string to, string subject, string body)
        {
            var apiKey     = _configuration["emailSettings:ResendApiKey"];
            var senderFrom = _configuration["emailSettings:ResendFrom"]
                             ?? "Akhbar Alyoum Academy <onboarding@resend.dev>";

            if (string.IsNullOrWhiteSpace(apiKey))
                return EmailResult.Failure("resend", "ResendApiKey is not configured in emailSettings.");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from    = senderFrom,
                to      = new[] { to },
                subject = subject,
                html    = body,
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.resend.com/emails", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Resend API response: {Response}", responseBody);
                return EmailResult.Success("resend");
            }

            _logger.LogError("Resend API error {StatusCode}: {Response}",
                (int)response.StatusCode, responseBody);

            return EmailResult.Failure("resend",
                $"Resend API returned {(int)response.StatusCode}: {responseBody}");
        }

        // ──────────────────────────────────────────────────────────────
        // Brevo Provider (HTTP API)
        // https://developers.brevo.com/reference/sendtransacemail
        // ──────────────────────────────────────────────────────────────
        private async Task<EmailResult> SendViaBrevoAsync(string to, string subject, string body)
        {
            var apiKey      = _configuration["emailSettings:BrevoApiKey"];
            var senderName  = _configuration["emailSettings:BrevoSenderName"] ?? "Akhbar Elyoum Academy";
            var senderEmail = _configuration["emailSettings:BrevoSenderEmail"];

            if (string.IsNullOrWhiteSpace(apiKey))
                return EmailResult.Failure("brevo", "BrevoApiKey is not configured in emailSettings.");

            if (string.IsNullOrWhiteSpace(senderEmail))
                return EmailResult.Failure("brevo", "BrevoSenderEmail is not configured in emailSettings.");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);
            client.DefaultRequestHeaders.Add("accept", "application/json");

            var payload = new
            {
                sender      = new { name = senderName, email = senderEmail },
                to          = new[] { new { email = to } },
                subject     = subject,
                htmlContent = body,
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Brevo API response: {Response}", responseBody);
                return EmailResult.Success("brevo");
            }

            _logger.LogError("Brevo API error {StatusCode}: {Response}",
                (int)response.StatusCode, responseBody);

            return EmailResult.Failure("brevo",
                $"Brevo API returned {(int)response.StatusCode}: {responseBody}");
        }

        // ──────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────
        private static string MaskEmail(string email)
        {
            var at = email.IndexOf('@');
            if (at <= 1) return "***@***";
            return email[0] + "***" + email[(at - 1)..];
        }
    }
}
