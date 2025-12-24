using System.Net.Http.Headers;
using System.Text.Json;

namespace TwitchAutoRecorder;

public sealed class TwitchApi : IDisposable
{
    private readonly HttpClient _http = new();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private string _clientId;
    private string _clientSecret;

    private string? _accessToken;
    private DateTimeOffset _tokenExpiresAtUtc;

    public TwitchApi(string clientId, string clientSecret)
    {
        _clientId = clientId ?? "";
        _clientSecret = clientSecret ?? "";
        _http.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<StreamSnapshot> GetStreamAsync(string userLogin, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userLogin))
            throw new ArgumentException("userLogin is required.");

        await EnsureTokenAsync(ct);

        using var req = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.twitch.tv/helix/streams?user_login={Uri.EscapeDataString(userLogin.Trim())}"
        );

        req.Headers.Add("Client-Id", _clientId);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        using var resp = await _http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var parsed = JsonSerializer.Deserialize<HelixStreamsResponse>(json, _jsonOptions)
                     ?? new HelixStreamsResponse();

        var s = parsed.Data.FirstOrDefault();
        if (s == null)
        {
            return new StreamSnapshot(false, "", userLogin, "", "", "", "", DateTimeOffset.MinValue);
        }

        return new StreamSnapshot(
            true, s.Id, s.UserLogin, s.UserName, s.GameId, s.GameName, s.Title,
            s.StartedAt.ToUniversalTime()
        );
    }

    private async Task EnsureTokenAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_clientId) || string.IsNullOrWhiteSpace(_clientSecret))
            throw new InvalidOperationException("Twitch ClientId/ClientSecret not set.");

        var now = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(_accessToken) && now < _tokenExpiresAtUtc.AddMinutes(-2))
            return;

        var url =
            "https://id.twitch.tv/oauth2/token" +
            $"?client_id={Uri.EscapeDataString(_clientId)}" +
            $"&client_secret={Uri.EscapeDataString(_clientSecret)}" +
            "&grant_type=client_credentials";

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        using var resp = await _http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var token = JsonSerializer.Deserialize<OAuthTokenResponse>(json, _jsonOptions)
                    ?? throw new InvalidOperationException("Failed to parse Twitch token response.");

        _accessToken = token.AccessToken;
        _tokenExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
    }

    public void Dispose() => _http.Dispose();
}
