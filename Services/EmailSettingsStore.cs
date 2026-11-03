using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace Alumni_Management_System.Services;

public record SavedEmailSettings(
    string Host,
    int Port,
    bool EnableSsl,
    string FromAddress,
    string FromName,
    string Username,
    string ProtectedPassword,
    string UpdatedBy,
    DateTime? UpdatedAt);

// SMTP settings an Admin can change from Email Settings, so a new mailbox or
// password doesn't need a code change or redeploy. Saved to
// App_Data/settings/smtp.json (excluded from git and publish, like Site
// Content). The password is encrypted with ASP.NET Data Protection - never
// written in plain text. With nothing saved, the "Smtp" section of
// appsettings.json / user-secrets is used, as before.
public class EmailSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _path;
    private readonly IDataProtector _protector;
    private readonly IOptionsMonitor<SmtpOptions> _configured;
    private readonly ILogger<EmailSettingsStore> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private SavedEmailSettings _cache;
    private bool _loaded;

    public EmailSettingsStore(IWebHostEnvironment environment, IDataProtectionProvider dataProtection, IOptionsMonitor<SmtpOptions> configured, ILogger<EmailSettingsStore> logger)
    {
        _path = Path.Combine(environment.ContentRootPath, "App_Data", "settings", "smtp.json");
        _protector = dataProtection.CreateProtector("Alumni_Management_System.SmtpPassword");
        _configured = configured;
        _logger = logger;
    }

    // What the admin saved, or null if nothing has been saved yet.
    public SavedEmailSettings GetSaved()
    {
        if (!_loaded)
        {
            _cache = Load();
            _loaded = true;
        }
        return _cache;
    }

    // The settings emails are actually sent with: the saved ones if any,
    // otherwise appsettings.json. A saved password that can no longer be
    // decrypted falls back to the configured one.
    public SmtpOptions GetEffective()
    {
        var configured = _configured.CurrentValue;
        var saved = GetSaved();
        if (saved == null)
        {
            return configured;
        }

        return new SmtpOptions
        {
            Host = saved.Host,
            Port = saved.Port,
            EnableSsl = saved.EnableSsl,
            FromAddress = saved.FromAddress,
            FromName = saved.FromName,
            Username = saved.Username,
            Password = Unprotect(saved.ProtectedPassword) ?? configured.Password
        };
    }

    // True when a password is available to send with (saved or configured).
    public bool HasPassword() => !string.IsNullOrEmpty(GetEffective().Password);

    // newPassword null or empty keeps the current password.
    public async Task SaveAsync(SmtpOptions settings, string newPassword, string changedBy)
    {
        await _writeLock.WaitAsync();
        try
        {
            var current = GetSaved();
            var protectedPassword = !string.IsNullOrEmpty(newPassword)
                ? _protector.Protect(newPassword)
                : current?.ProtectedPassword ?? ProtectConfiguredPassword();

            var entry = new SavedEmailSettings(
                settings.Host?.Trim(),
                settings.Port,
                settings.EnableSsl,
                settings.FromAddress?.Trim(),
                settings.FromName?.Trim(),
                settings.Username?.Trim(),
                protectedPassword,
                changedBy,
                DateTime.Now);

            Directory.CreateDirectory(Path.GetDirectoryName(_path));

            // Write then swap, so a crash mid-write can't leave half a file.
            var tempPath = _path + ".tmp";
            await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(entry, JsonOptions));
            File.Move(tempPath, _path, overwrite: true);

            _cache = entry;
            _loaded = true;
        }
        finally
        {
            _writeLock.Release();
        }

        _logger.LogInformation("Email (SMTP) settings were updated by '{User}'.", changedBy);
    }

    // Goes back to the appsettings.json values.
    public async Task ResetAsync(string changedBy)
    {
        await _writeLock.WaitAsync();
        try
        {
            File.Delete(_path);
            _cache = null;
            _loaded = true;
        }
        finally
        {
            _writeLock.Release();
        }

        _logger.LogInformation("Email (SMTP) settings were reset to the configured defaults by '{User}'.", changedBy);
    }

    // So saving other fields the first time doesn't drop a password that so
    // far only lived in user-secrets / environment variables.
    private string ProtectConfiguredPassword()
    {
        var configuredPassword = _configured.CurrentValue.Password;
        return string.IsNullOrEmpty(configuredPassword) ? null : _protector.Protect(configuredPassword);
    }

    private string Unprotect(string protectedPassword)
    {
        if (string.IsNullOrEmpty(protectedPassword))
        {
            return null;
        }

        try
        {
            return _protector.Unprotect(protectedPassword);
        }
        catch (CryptographicException ex)
        {
            // e.g. the server's Data Protection keys were lost or replaced.
            _logger.LogWarning(ex, "The saved SMTP password couldn't be decrypted; re-enter it in Email Settings.");
            return null;
        }
    }

    private SavedEmailSettings Load()
    {
        if (!File.Exists(_path))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SavedEmailSettings>(File.ReadAllText(_path));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Couldn't read email settings file '{Path}'; using appsettings.json.", _path);
            return null;
        }
    }
}
