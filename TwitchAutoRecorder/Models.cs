using System.Text.Json.Serialization;

namespace TwitchAutoRecorder;

public sealed class HelixStreamsResponse
{
    [JsonPropertyName("data")]
    public StreamData[] Data { get; set; } = Array.Empty<StreamData>();
}

public sealed class StreamData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("user_login")]
    public string UserLogin { get; set; } = "";

    [JsonPropertyName("user_name")]
    public string UserName { get; set; } = "";

    [JsonPropertyName("game_id")]
    public string GameId { get; set; } = "";

    [JsonPropertyName("game_name")]
    public string GameName { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("started_at")]
    public DateTimeOffset StartedAt { get; set; }
}

public sealed class OAuthTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "";
}

public sealed record StreamSnapshot(
    bool IsLive,
    string StreamId,
    string UserLogin,
    string UserName,
    string GameId,
    string GameName,
    string Title,
    DateTimeOffset StartedAtUtc
);
