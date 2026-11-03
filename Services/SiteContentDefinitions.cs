namespace Alumni_Management_System.Services;

public enum SiteContentKind
{
    // Markdown rendered as a page section (Privacy, Help).
    Page,
    // Markdown email body (+ subject line unless DefaultSubject is null).
    Email,
    // A single line of text, shown as-is (footer).
    PlainText
}

// A {{Name}} token an admin can use in a piece of content. Sample is what the
// email preview fills it with; Required ones can't be deleted on save, or the
// email stops doing its job (e.g. a password reset without the password).
public record ContentPlaceholder(string Name, string Description, string Sample = null, bool Required = false);

public class SiteContentDefinition
{
    public string Key { get; init; }
    public string Title { get; init; }
    public string Group { get; init; }
    public string Description { get; init; }
    public SiteContentKind Kind { get; init; }
    public string DefaultSubject { get; init; }
    public string DefaultBody { get; init; }
    public IReadOnlyList<ContentPlaceholder> Placeholders { get; init; } = Array.Empty<ContentPlaceholder>();

    public bool HasSubject => DefaultSubject != null;
}

// Every piece of text admins can edit from Site Content. The defaults are the
// text the app shipped with - used until an admin saves their own version, and
// again after "Reset to default".
public static class SiteContentDefinitions
{
    public const string PrivacyPage = "privacy";
    public const string HelpPage = "help";
    public const string FooterTagline = "footer-tagline";
    public const string FooterCopyright = "footer-copyright";
    public const string AccountCreatedEmail = "email-account-created";
    public const string AdminPasswordResetEmail = "email-admin-password-reset";
    public const string SelfPasswordResetEmail = "email-self-password-reset";
    public const string TwoFactorCodeEmail = "email-2fa-code";
    public const string BulkMessageEmail = "email-bulk-message";

    // Filled from appsettings.json, so pages show the current contacts.
    private static readonly ContentPlaceholder[] SitePlaceholders =
    {
        new("AdminContactEmail", "System Administrator contact email (from site settings)"),
        new("SocAdminContactEmail", "School of Computing Administrator contact email (from site settings)"),
        new("Year", "The current year")
    };

    public static readonly IReadOnlyList<SiteContentDefinition> All = new List<SiteContentDefinition>
    {
        new()
        {
            Key = PrivacyPage,
            Title = "Privacy Policy",
            Group = "Pages",
            Description = "The Privacy page, linked from the sidebar and footer.",
            Kind = SiteContentKind.Page,
            Placeholders = SitePlaceholders,
            DefaultBody =
@"## 1. Introduction
The Alumni Management System is committed to protecting the privacy and personal information of all alumni, students, and Administrators who use this platform. This policy explains how we collect, use, and safeguard your information.

## 2. Information We Collect
- Personal details (Name, Email, Phone, Address)
- Academic information (Degree, Graduation Year, Program)
- Professional information (Employment, Organization)
- System activity (Login, Messages, Updates)

## 3. How We Use Your Information
- To manage alumni records and communication
- To provide networking and professional opportunities
- To improve system functionality and reporting
- To ensure system security and authentication

## 4. Data Protection
We implement industry-standard security measures including authentication, encrypted communication, and controlled access to ensure your data is protected. Only authorized Administrators can access sensitive information.

## 5. Data Sharing
Your personal data is never sold or shared with third parties. Information is only used within the Alumni Management System for official University purposes.

## 6. Your Rights
- You may update your profile at any time
- You may request data correction
- You may request account deactivation

## 7. Contact
For privacy-related concerns, please reach out to one of the contacts below or the University IT department.

- **System Administrator:** [{{AdminContactEmail}}](mailto:{{AdminContactEmail}})
- **School of Computing (SOC) Administrator:** [{{SocAdminContactEmail}}](mailto:{{SocAdminContactEmail}})
- **Mailing Address:** University of South Alabama, 307 N University Blvd, Mobile, AL 36688"
        },
        new()
        {
            Key = HelpPage,
            Title = "Help & Support",
            Group = "Pages",
            Description = "The Help page. Text before the first \"## \" heading is the welcome box; each \"## \" heading becomes a question you can expand.",
            Kind = SiteContentKind.Page,
            Placeholders = SitePlaceholders,
            DefaultBody =
@"Welcome to the Alumni Management System Help Center. Here you can learn how to use the platform.

## How do I update my profile?
Go to Profile from the top menu and update your details.

## How do Admins manage alumni?
Admin users can manage alumni records using the sidebar navigation.

## Who do I contact for support?
System Administrator: [{{AdminContactEmail}}](mailto:{{AdminContactEmail}})
School of Computing (SOC) Administrator: [{{SocAdminContactEmail}}](mailto:{{SocAdminContactEmail}})
Phone: +1 (251) 000-0000"
        },
        new()
        {
            Key = FooterTagline,
            Title = "Footer tagline",
            Group = "Footer",
            Description = "The line under \"Alumni Management System\" in the footer.",
            Kind = SiteContentKind.PlainText,
            Placeholders = SitePlaceholders,
            DefaultBody = "Connecting Alumni • Building Futures"
        },
        new()
        {
            Key = FooterCopyright,
            Title = "Footer copyright",
            Group = "Footer",
            Description = "The copyright line on the right of the footer.",
            Kind = SiteContentKind.PlainText,
            Placeholders = SitePlaceholders,
            DefaultBody = "© {{Year}} AMS"
        },
        new()
        {
            Key = AccountCreatedEmail,
            Title = "New account",
            Group = "Emails",
            Description = "Sent when an Admin creates an Admin or Staff account.",
            Kind = SiteContentKind.Email,
            Placeholders = new ContentPlaceholder[]
            {
                new("Role", "The account's role (Admin or Staff)", "Staff"),
                new("UserName", "The temporary username", "tmp_ab12cd", Required: true),
                new("TempPassword", "The temporary password", "Xy7#kQ2m!p", Required: true)
            },
            DefaultSubject = "Your Alumni Management System account",
            DefaultBody =
@"A {{Role}} account has been created for you.

**Temporary username:** {{UserName}}
**Temporary password:** {{TempPassword}}

Log in with these at the site, then you'll be asked to choose your own username and password."
        },
        new()
        {
            Key = AdminPasswordResetEmail,
            Title = "Password reset by an Admin",
            Group = "Emails",
            Description = "Sent when an Admin resets someone's password from Manage Users.",
            Kind = SiteContentKind.Email,
            Placeholders = new ContentPlaceholder[]
            {
                new("UserName", "The user's username", "jdoe", Required: true),
                new("TempPassword", "The temporary password", "Xy7#kQ2m!p", Required: true)
            },
            DefaultSubject = "Your password has been reset - Alumni Management System",
            DefaultBody =
@"Your administrator reset your password.

**Temporary password:** {{TempPassword}}

Log in with your existing username (**{{UserName}}**) and this temporary password - you'll be asked to set your own right after."
        },
        new()
        {
            Key = SelfPasswordResetEmail,
            Title = "Forgot password",
            Group = "Emails",
            Description = "Sent when someone uses Forgot Password on the login page.",
            Kind = SiteContentKind.Email,
            Placeholders = new ContentPlaceholder[]
            {
                new("UserName", "The user's username", "jdoe", Required: true),
                new("TempPassword", "The temporary password", "Xy7#kQ2m!p", Required: true)
            },
            DefaultSubject = "Your temporary password - Alumni Management System",
            DefaultBody =
@"You requested a password reset.

**Username:** {{UserName}}
**Temporary password:** {{TempPassword}}

Log in with these - you'll be asked to set your own password right after.

Didn't request this? Contact your Administrator."
        },
        new()
        {
            Key = TwoFactorCodeEmail,
            Title = "Two-factor login code",
            Group = "Emails",
            Description = "Sent at login to users who turned on two-factor authentication.",
            Kind = SiteContentKind.Email,
            Placeholders = new ContentPlaceholder[]
            {
                new("Code", "The verification code", "482913", Required: true)
            },
            DefaultSubject = "Your Alumni Management System verification code",
            DefaultBody =
@"Your verification code is: **{{Code}}**

This code expires shortly - if it's expired, request a new one from the login page."
        },
        new()
        {
            Key = BulkMessageEmail,
            Title = "Message to alumni (wrapper)",
            Group = "Emails",
            Description = "The greeting and signature around every message sent from Messages. The subject is each message's own title.",
            Kind = SiteContentKind.Email,
            Placeholders = new ContentPlaceholder[]
            {
                new("Body", "The message text", "We're hosting an alumni mixer on campus next month.\nHope to see you there!", Required: true)
            },
            DefaultBody =
@"Hello,

{{Body}}

Thanks,
Alumni Management System,
University of South Alabama"
        }
    };

    public static SiteContentDefinition Find(string key) =>
        All.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase));
}
