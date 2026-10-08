using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QuizApp.Services;

public record GeminiModelInfo(
    string Name,
    string DisplayName,
    int InputTokenLimit,
    int OutputTokenLimit);

public interface IGeminiModelCatalog
{
    /// <summary>
    /// All text-generation Gemini models available to the configured API key,
    /// ranked so current Flash models are tried first.
    /// </summary>
    Task<IReadOnlyList<GeminiModelInfo>> GetModelsAsync(
        bool forceRefresh = false,
        CancellationToken ct = default);

    /// <summary>
    /// True when a model recently failed and should be tried later.
    /// </summary>
    bool IsCoolingDown(string modelName);

    /// <summary>
    /// Put a model into temporary cooldown after a failure.
    /// </summary>
    void Cooldown(string modelName, TimeSpan duration);
}

/// <summary>
/// Small helpers shared by Gemini services.
/// </summary>
internal static class GeminiApi
{
    public const string BaseUrl =
        "https://generativelanguage.googleapis.com/v1beta";

    public static (string status, string message) ReadError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("error", out var err))
            {
                var status =
                    err.TryGetProperty("status", out var s)
                        ? s.GetString() ?? ""
                        : "";

                var message =
                    err.TryGetProperty("message", out var m)
                        ? m.GetString() ?? ""
                        : "";

                return (status, message);
            }
        }
        catch
        {
            // Response was not valid JSON.
        }

        return ("", body.Length > 500 ? body[..500] : body);
    }

    /// <summary>
    /// Problems with the API key/project itself.
    /// Trying another model cannot fix these.
    /// </summary>
    public static bool IsKeyProblem(int http, string message)
    {
        if (http == 401)
            return true;

        if (http is not (400 or 403))
            return false;

        var m = message.ToLowerInvariant();

        return
            m.Contains("api key") ||
            m.Contains("api_key") ||
            m.Contains("leaked") ||
            m.Contains("has not been used") ||
            m.Contains("suspended") ||
            (http == 403 && m.Contains("project"));
    }
}

/// <summary>
/// Dynamically loads Gemini models available through models.list.
/// Models that are unsuitable for quiz generation are filtered out.
/// </summary>
public class GeminiModelCatalog : IGeminiModelCatalog
{
    private static readonly string[] ExcludedKeywords =
    [
        "embedding",
        "imagen",
        "veo",
        "tts",
        "image",
        "live",
        "audio",
        "native",
        "lyria",
        "robotics",
        "computer-use",
        "aqa",
        "omni",
        "transcribe",
        "translate",
        "learnlm",
        "vision",
        "deep-research"
    ];

    private static readonly TimeSpan CacheTtl =
        TimeSpan.FromHours(1);

    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiModelCatalog> _log;

    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly ConcurrentDictionary<string, DateTime> _cooldowns = new();

    private IReadOnlyList<GeminiModelInfo>? _cache;
    private DateTime _cacheTime;

    public GeminiModelCatalog(
        IHttpClientFactory http,
        IConfiguration config,
        ILogger<GeminiModelCatalog> log)
    {
        _http = http;
        _config = config;
        _log = log;
    }

    public bool IsCoolingDown(string modelName)
    {
        if (_cooldowns.TryGetValue(modelName, out var until))
        {
            if (until > DateTime.UtcNow)
                return true;

            _cooldowns.TryRemove(modelName, out _);
        }

        return false;
    }

    public void Cooldown(string modelName, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            return;

        _cooldowns[modelName] =
            DateTime.UtcNow + duration;
    }

    public async Task<IReadOnlyList<GeminiModelInfo>> GetModelsAsync(
        bool forceRefresh = false,
        CancellationToken ct = default)
    {
        if (!forceRefresh &&
            _cache != null &&
            DateTime.UtcNow - _cacheTime < CacheTtl)
        {
            return _cache;
        }

        await _lock.WaitAsync(ct);

        try
        {
            if (!forceRefresh &&
                _cache != null &&
                DateTime.UtcNow - _cacheTime < CacheTtl)
            {
                return _cache;
            }

            try
            {
                var models = await LoadAsync(ct);

                _cache = models;
                _cacheTime = DateTime.UtcNow;

                return models;
            }
            catch (QuizGenerationException) when (_cache != null)
            {
                _log.LogWarning(
                    "Gemini model refresh failed. Using cached model list.");

                return _cache;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IReadOnlyList<GeminiModelInfo>> LoadAsync(
        CancellationToken ct)
    {
        var apiKey = _config["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new QuizGenerationException(
                "AI generation is not configured. Set Gemini:ApiKey in appsettings.json and restart the app.",
                503);
        }

        var client = _http.CreateClient("gemini");

        var found = new List<GeminiModelInfo>();

        string? pageToken = null;

        do
        {
            var url =
                $"{GeminiApi.BaseUrl}/models?pageSize=1000" +
                (pageToken != null
                    ? "&pageToken=" +
                      Uri.EscapeDataString(pageToken)
                    : "");

            using var msg =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url);

            msg.Headers.Add(
                "x-goog-api-key",
                apiKey);

            using var cts =
                CancellationTokenSource.CreateLinkedTokenSource(ct);

            cts.CancelAfter(
                TimeSpan.FromSeconds(30));

            int http;
            string body;

            try
            {
                using var res =
                    await client.SendAsync(
                        msg,
                        cts.Token);

                http = (int)res.StatusCode;

                body =
                    await res.Content.ReadAsStringAsync(
                        cts.Token);
            }
            catch (OperationCanceledException)
                when (!ct.IsCancellationRequested)
            {
                throw new QuizGenerationException(
                    "Timed out while loading the Gemini model list.",
                    504);
            }
            catch (HttpRequestException ex)
            {
                _log.LogError(
                    ex,
                    "Could not reach the Gemini API.");

                throw new QuizGenerationException(
                    "Could not reach the Gemini API. Check the server's internet connection.",
                    502);
            }

            if (http is < 200 or >= 300)
            {
                var (_, message) =
                    GeminiApi.ReadError(body);

                _log.LogError(
                    "Gemini models.list failed {Status}: {Body}",
                    http,
                    body);

                throw new QuizGenerationException(
                    GeminiApi.IsKeyProblem(
                        http,
                        message)
                        ? $"The Gemini API key was rejected: {message}"
                        : $"Could not load the Gemini model list (HTTP {http}): {message}",
                    503);
            }

            using var doc =
                JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty(
                    "models",
                    out var arr) &&
                arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var m in arr.EnumerateArray())
                {
                    var info = ToModelInfo(m);

                    if (info != null)
                        found.Add(info);
                }
            }

            pageToken =
                doc.RootElement.TryGetProperty(
                    "nextPageToken",
                    out var token)
                    ? token.GetString()
                    : null;

        } while (!string.IsNullOrEmpty(pageToken));

        if (found.Count == 0)
        {
            throw new QuizGenerationException(
                "This API key has no Gemini models that support text generation.",
                503);
        }

        var ranked =
            found
                .GroupBy(
                    x => x.Name,
                    StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .Select(m => (m, key: SortKey(m.Name)))
                .OrderBy(x => x.key.tier)
                .ThenByDescending(x => x.key.version)
                .ThenBy(x => x.key.preview)
                .ThenBy(
                    x => x.m.Name,
                    StringComparer.OrdinalIgnoreCase)
                .Select(x => x.m)
                .ToList();

        _log.LogInformation(
            "Loaded {Count} Gemini models: {Names}",
            ranked.Count,
            string.Join(
                ", ",
                ranked.Select(x => x.Name)));

        return ranked;
    }

    private static GeminiModelInfo? ToModelInfo(
        JsonElement m)
    {
        var full =
            m.TryGetProperty(
                "name",
                out var n)
                ? n.GetString() ?? ""
                : "";

        var name =
            full.StartsWith(
                "models/",
                StringComparison.OrdinalIgnoreCase)
                ? full[7..]
                : full;

        if (name.Length == 0)
            return null;

        if (!name.StartsWith(
                "gemini",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var lower =
            name.ToLowerInvariant();

        if (ExcludedKeywords.Any(
                k => lower.Contains(k)))
        {
            return null;
        }

        var supportsGenerate = false;

        if (m.TryGetProperty(
                "supportedGenerationMethods",
                out var methods) &&
            methods.ValueKind == JsonValueKind.Array)
        {
            supportsGenerate =
                methods
                    .EnumerateArray()
                    .Any(x =>
                        string.Equals(
                            x.GetString(),
                            "generateContent",
                            StringComparison.OrdinalIgnoreCase));
        }

        if (!supportsGenerate)
            return null;

        var display =
            m.TryGetProperty(
                "displayName",
                out var d)
                ? d.GetString() ?? name
                : name;

        var input =
            m.TryGetProperty(
                "inputTokenLimit",
                out var i) &&
            i.TryGetInt32(out var iv)
                ? iv
                : 0;

        var output =
            m.TryGetProperty(
                "outputTokenLimit",
                out var o) &&
            o.TryGetInt32(out var ov)
                ? ov
                : 0;

        return new GeminiModelInfo(
            name,
            display,
            input,
            output);
    }

    /// <summary>
    /// Current Flash models first,
    /// then Pro,
    /// then Flash-Lite.
    /// Newer versions first.
    /// Stable versions before preview.
    /// </summary>
    private static (
        int tier,
        double version,
        int preview) SortKey(
        string name)
    {
        var lower =
            name.ToLowerInvariant();

        var tier =
            lower.Contains("flash-lite")
                ? 2
                : lower.Contains("flash")
                    ? 0
                    : lower.Contains("pro")
                        ? 1
                        : 3;

        var match =
            Regex.Match(
                lower,
                @"gemini-(\d+(?:\.\d+)?)");

        var version =
            match.Success
                ? double.Parse(
                    match.Groups[1].Value,
                    CultureInfo.InvariantCulture)
                : 0;

        var preview =
            lower.Contains("preview") ||
            lower.Contains("exp")
                ? 1
                : 0;

        return (
            tier,
            version,
            preview);
    }
}