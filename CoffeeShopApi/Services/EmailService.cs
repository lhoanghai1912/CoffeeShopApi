using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace CoffeeShopApi.Services;

#region Email Settings

/// <summary>
/// Cấu hình SMTP để gửi email
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    // public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpUsername { get; set; } = string.Empty;

    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "CoffeeShop";
    public bool EnableSsl { get; set; } = true;
    public bool IsDevelopment { get; set; } =  false;
    /// <summary>
    /// Nếu true, chỉ log email ra console thay vì gửi thật (cho dev/test)
    /// </summary>
}

#endregion

#region Interface

public interface IEmailService
{
    /// <summary>
    /// Gửi mã xác thực email khi đăng ký
    /// </summary>
    Task<bool> SendWelcomeEmailAsync(string toEmail, string fullName);
    // Task<bool> SendEmailVerificationCodeAsync(string toEmail, string fullName, string verificationCode);
    
    /// <summary>
    /// Gửi mã reset password
    /// </summary>
    Task<bool> SendPasswordResetCodeAsync(string toEmail, string fullName, string resetCode);
    
    /// <summary>
    /// Gửi thông báo password đã được đổi
    /// </summary>
    Task<bool> SendPasswordChangedNotificationAsync(string toEmail, string fullName);
    
    /// <summary>
    /// Gửi email generic
    /// </summary>
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
}

#endregion

#region Implementation

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        // Bind EmailSettings from configuration section "EmailSettings"
        var section = configuration.GetSection("EmailSettings");
        var settings = section.Get<EmailSettings>();
        _settings = settings ?? new EmailSettings();
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string fullName)
    {
        var subject = "Đăng kí tài khoản CoffeeShop";
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #8B4513;'>☕ Chào mừng đến với CoffeeShop!</h2>
                <p>Xin chào <strong>{fullName ?? "bạn"}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản. Chúc bạn có trải nghiệm sử dụng vui vẻ:</p>
                <div style='background-color: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;'>
                </div>
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px;'>© 2024 CoffeeShop. All rights reserved.</p>
            </div>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }
    
    public async Task<bool> SendEmailVerificationCodeAsync(string toEmail, string fullName, string verificationCode)
    {
        var subject = "Xác thực tài khoản CoffeeShop";
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #8B4513;'>☕ Chào mừng đến với CoffeeShop!</h2>
                <p>Xin chào <strong>{fullName ?? "bạn"}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản. Vui lòng sử dụng mã sau để xác thực email:</p>
                <div style='background-color: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;'>
                    <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #8B4513;'>{verificationCode}</span>
                </div>
                <p style='color: #666;'>⏰ Mã này có hiệu lực trong <strong>15 phút</strong>.</p>
                <p style='color: #666;'>Nếu bạn không yêu cầu đăng ký, vui lòng bỏ qua email này.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px;'>© 2024 CoffeeShop. All rights reserved.</p>
            </div>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendPasswordResetCodeAsync(string toEmail, string fullName, string resetCode)
    {
        var subject = "Đặt lại mật khẩu CoffeeShop";
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #8B4513;'>🔐 Đặt lại mật khẩu</h2>
                <p>Xin chào <strong>{fullName ?? "bạn"}</strong>,</p>
                <p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng sử dụng mã sau:</p>
                <div style='background-color: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;'>
                    <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #8B4513;'>{resetCode}</span>
                </div>
                <p style='color: #666;'>⏰ Mã này có hiệu lực trong <strong>30 phút</strong>.</p>
                <p style='color: #cc0000;'>⚠️ Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng đổi mật khẩu ngay và liên hệ hỗ trợ.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px;'>© 2024 CoffeeShop. All rights reserved.</p>
            </div>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendPasswordChangedNotificationAsync(string toEmail, string fullName)
    {
        var subject = "Mật khẩu đã được thay đổi";
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #8B4513;'>✅ Mật khẩu đã được thay đổi</h2>
                <p>Xin chào <strong>{fullName ?? "bạn"}</strong>,</p>
                <p>Mật khẩu tài khoản CoffeeShop của bạn vừa được thay đổi thành công.</p>
                <p>Thời gian: <strong>{DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</strong></p>
                <p style='color: #cc0000;'>⚠️ Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ hỗ trợ ngay lập tức.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #999; font-size: 12px;'>© 2024 CoffeeShop. All rights reserved.</p>
            </div>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            // Nếu UseFakeEmail = true, chỉ log ra console
            if (_settings.IsDevelopment)
            {
                _logger.LogInformation("========== FAKE EMAIL ==========");
                _logger.LogInformation("To: {ToEmail}", toEmail);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body: {Body}", htmlBody);
                _logger.LogInformation("================================");
                return true;
            }

            // Gửi email thật
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            return false;
        }
    }
}

#endregion
