namespace PolyhydraGames.Twitch.Services;

public class TwitchWebUrl(IConfiguration config) : ITwitchWebUrl
{
    public string Url { get; set; } = config["Twitch:StreamUrl"];
}