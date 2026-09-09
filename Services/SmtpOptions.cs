namespace Alumni_Management_System.Services;

// Bound from the "Smtp" configuration section. Host/Port/FromAddress/FromName/
// Username/EnableSsl are fine to keep in appsettings.json; Password must come
// from user-secrets locally (dotnet user-secrets) or an environment variable /
// secret store in production - never committed to source control.
public class SmtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromAddress { get; set; }
    public string FromName { get; set; } = "Alumni Management System";
    public string Username { get; set; }
    public string Password { get; set; }
}
