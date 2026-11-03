using System.Net;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Html;

namespace Alumni_Management_System.Services;

public record RenderedEmail(string Subject, string HtmlBody);

public record HelpSection(string Question, HtmlString Answer);

// Turns stored content into HTML. Admins write Markdown; raw HTML in it is
// shown as text (DisableHtml), so a saved page can't inject scripts.
public class SiteContentRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseAutoLinks()
        // An Enter in the editor is a line break on the page, as people expect.
        .UseSoftlineBreakAsHardlineBreak()
        .DisableHtml()
        .Build();

    private static readonly Regex PlaceholderPattern = new(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.Compiled);

    private readonly SiteContentStore _store;
    private readonly IConfiguration _configuration;

    public SiteContentRenderer(SiteContentStore store, IConfiguration configuration)
    {
        _store = store;
        _configuration = configuration;
    }

    // ---------- Pages ----------
    // Page placeholders are site settings (trusted), so they're filled in
    // before the Markdown is rendered - that lets them sit inside links.

    public HtmlString RenderPage(string key) => new(RenderPageMarkdown(_store.Get(key).Body));

    public string RenderPageMarkdown(string markdown) =>
        ToHtml(Fill(markdown, SiteValues(), encode: false));

    public string RenderText(string key) => RenderPlainText(_store.Get(key).Body);

    public string RenderPlainText(string text) => Fill(text, SiteValues(), encode: false);

    // Help page: text before the first "## " heading is the intro, and each
    // "## " heading is a question whose answer is everything up to the next one.
    public (HtmlString Intro, IReadOnlyList<HelpSection> Sections) RenderHelp(string markdown)
    {
        var intro = new List<string>();
        var sections = new List<(string Question, List<string> Lines)>();

        foreach (var line in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            if (line.StartsWith("## "))
            {
                sections.Add((line[3..].Trim(), new List<string>()));
            }
            else if (sections.Count == 0)
            {
                intro.Add(line);
            }
            else
            {
                sections[^1].Lines.Add(line);
            }
        }

        var siteValues = SiteValues();
        return (
            new HtmlString(RenderPageMarkdown(string.Join("\n", intro))),
            sections.Select(s => new HelpSection(
                Fill(s.Question, siteValues, encode: false),
                new HtmlString(RenderPageMarkdown(string.Join("\n", s.Lines))))).ToList());
    }

    // ---------- Emails ----------
    // Email values are user data (usernames, generated passwords, message
    // text), so they're HTML-encoded and filled in after the Markdown is
    // rendered - a "*" or "<" in a password can't turn into formatting or HTML.

    public RenderedEmail RenderEmail(string key, IDictionary<string, string> values)
    {
        var entry = _store.Get(key);
        return RenderEmail(entry.Subject, entry.Body, values);
    }

    public RenderedEmail RenderEmail(string subjectTemplate, string bodyTemplate, IDictionary<string, string> values)
    {
        var subject = subjectTemplate == null ? null : Fill(subjectTemplate, values, encode: false).ReplaceLineEndings(" ");
        var html = Fill(ToHtml(bodyTemplate ?? ""), values, encode: true);
        return new RenderedEmail(subject, html);
    }

    // ---------- Validation ----------

    public List<string> Validate(SiteContentDefinition definition, string subject, string body)
    {
        var errors = new List<string>();
        var text = (subject ?? "") + "\n" + (body ?? "");
        var used = PlaceholderPattern.Matches(text).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);

        foreach (var name in used.Where(n => definition.Placeholders.All(p => p.Name != n)))
        {
            errors.Add($"{{{{{name}}}}} isn't a placeholder this content can use - check the spelling (it's case-sensitive).");
        }
        foreach (var placeholder in definition.Placeholders.Where(p => p.Required && !used.Contains(p.Name)))
        {
            errors.Add($"{{{{{placeholder.Name}}}}} must stay in this email - it's {char.ToLowerInvariant(placeholder.Description[0])}{placeholder.Description[1..]}.");
        }
        return errors;
    }

    // DisableHtml stops raw tags, but Markdown links can still say
    // [x](javascript:...) - only web, email and on-site links are kept.
    private static string ToHtml(string markdown)
    {
        var document = Markdown.Parse(markdown, Pipeline);
        foreach (var link in document.Descendants<LinkInline>())
        {
            if (!IsSafeUrl(link.Url))
            {
                link.Url = "#";
            }
        }
        foreach (var autolink in document.Descendants<AutolinkInline>())
        {
            if (!autolink.IsEmail && !IsSafeUrl(autolink.Url))
            {
                autolink.Url = "#";
            }
        }
        return document.ToHtml(Pipeline);
    }

    private static bool IsSafeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.StartsWith('/') || url.StartsWith('#'))
        {
            return true;
        }
        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            ? uri.Scheme is "http" or "https" or "mailto"
            // Relative ("help", "../x") has no scheme - anything with a ":" does.
            : !url.Contains(':');
    }

    private Dictionary<string, string> SiteValues() => new()
    {
        ["AdminContactEmail"] = _configuration["AdminContactEmail"],
        ["SocAdminContactEmail"] = _configuration["SocAdminContactEmail"],
        ["Year"] = DateTime.Now.Year.ToString()
    };

    // Unknown placeholders are left as typed, so a mistake is visible rather
    // than silently blank.
    private static string Fill(string template, IDictionary<string, string> values, bool encode) =>
        PlaceholderPattern.Replace(template ?? "", match =>
        {
            if (!values.TryGetValue(match.Groups[1].Value, out var value))
            {
                return match.Value;
            }
            value ??= "";
            return encode
                ? string.Join("<br/>", value.ReplaceLineEndings("\n").Split('\n').Select(WebUtility.HtmlEncode))
                : value;
        });
}
