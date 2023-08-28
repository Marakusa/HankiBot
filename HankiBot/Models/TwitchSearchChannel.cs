using Newtonsoft.Json;

namespace HankiBot.Models;

public class TwitchSearchChannelData
{
    [JsonProperty("broadcaster_language")]
    public string BroadcasterLanguage { get; set; }

    [JsonProperty("broadcaster_login")]
    public string BroadcasterLogin { get; set; }

    [JsonProperty("display_name")]
    public string DisplayName { get; set; }

    [JsonProperty("game_id")]
    public string GameId { get; set; }

    [JsonProperty("game_name")]
    public string GameName { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("is_live")]
    public bool IsLive { get; set; }

    [JsonProperty("tag_ids")]
    public List<object> TagIds { get; set; }

    [JsonProperty("tags")]
    public List<object> Tags { get; set; }

    [JsonProperty("thumbnail_url")]
    public string ThumbnailUrl { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("started_at")]
    public string StartedAt { get; set; }
}

public class Pagination
{
    [JsonProperty("cursor")]
    public string Cursor { get; set; }
}

public class TwitchSearchChannel
{
    [JsonProperty("data")]
    public List<TwitchSearchChannelData> Data { get; set; }

    [JsonProperty("pagination")]
    public Pagination Pagination { get; set; }
}