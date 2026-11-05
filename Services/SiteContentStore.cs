using System.Collections.Concurrent;
using System.Text.Json;

namespace Alumni_Management_System.Services;

public record SiteContentEntry(string Subject, string Body, string UpdatedBy, DateTime? UpdatedAt)
{
    public bool IsCustomized => UpdatedAt != null;
}

// Admin-edited content, one JSON file per key under App_Data/content (no
// database table). A missing or unreadable file falls back to the definition's
// default text, so the site and its emails keep working either way. Every save
// or reset first copies the previous file into App_Data/content/history.
//
// App_Data is excluded from publish and from git (see the .csproj and
// .gitignore), so a deploy doesn't overwrite what the admins wrote.
public class SiteContentStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _directory;
    private readonly ILogger<SiteContentStore> _logger;
    private readonly ConcurrentDictionary<string, SiteContentEntry> _cache = new();
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public SiteContentStore(IWebHostEnvironment environment, ILogger<SiteContentStore> logger)
    {
        _directory = Path.Combine(environment.ContentRootPath, "App_Data", "content");
        _logger = logger;
    }

    public SiteContentEntry Get(string key)
    {
        var definition = RequireDefinition(key);
        return _cache.GetOrAdd(definition.Key, _ => Load(definition));
    }

    public async Task SaveAsync(string key, string subject, string body, string changedBy)
    {
        var definition = RequireDefinition(key);
        var entry = new SiteContentEntry(definition.HasSubject ? subject : null, body, changedBy, DateTime.Now);

        await _writeLock.WaitAsync();
        try
        {
            Directory.CreateDirectory(_directory);
            var path = PathFor(definition.Key);
            BackUp(definition.Key, path);

            // Write then swap, so a crash mid-write can't leave half a file.
            var tempPath = path + ".tmp";
            await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(entry, JsonOptions));
            File.Move(tempPath, path, overwrite: true);

            _cache[definition.Key] = entry;
        }
        finally
        {
            _writeLock.Release();
        }

        _logger.LogInformation("Site content '{Key}' was updated by '{User}'.", definition.Key, changedBy);
    }

    public async Task ResetAsync(string key, string changedBy)
    {
        var definition = RequireDefinition(key);

        await _writeLock.WaitAsync();
        try
        {
            var path = PathFor(definition.Key);
            BackUp(definition.Key, path);
            File.Delete(path);
            _cache.TryRemove(definition.Key, out _);
        }
        finally
        {
            _writeLock.Release();
        }

        _logger.LogInformation("Site content '{Key}' was reset to its default by '{User}'.", definition.Key, changedBy);
    }

    private SiteContentEntry Load(SiteContentDefinition definition)
    {
        var defaults = new SiteContentEntry(definition.DefaultSubject, definition.DefaultBody, null, null);
        var path = PathFor(definition.Key);
        if (!File.Exists(path))
        {
            return defaults;
        }

        try
        {
            var saved = JsonSerializer.Deserialize<SiteContentEntry>(File.ReadAllText(path));
            if (saved == null || string.IsNullOrWhiteSpace(saved.Body))
            {
                return defaults;
            }
            // Keep the default subject if the file has none (e.g. hand-edited).
            return definition.HasSubject && string.IsNullOrWhiteSpace(saved.Subject)
                ? saved with { Subject = definition.DefaultSubject }
                : saved;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Couldn't read site content file '{Path}'; using the default text.", path);
            return defaults;
        }
    }

    private void BackUp(string key, string path)
    {
        if (!File.Exists(path))
        {
            return;
        }
        var historyDirectory = Path.Combine(_directory, "history");
        Directory.CreateDirectory(historyDirectory);
        File.Copy(path, Path.Combine(historyDirectory, $"{key}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.json"), overwrite: true);
    }

    private string PathFor(string key) => Path.Combine(_directory, key + ".json");

    // Keys only ever come from SiteContentDefinitions, never straight from a
    // request, so they can't point the file path anywhere else.
    private static SiteContentDefinition RequireDefinition(string key) =>
        SiteContentDefinitions.Find(key) ?? throw new ArgumentException($"Unknown site content key '{key}'.", nameof(key));
}
