using TwitchLib.Api.Helix;

namespace PolyhydraGames.Twitch.Interfaces;

public interface IStreamerTwitchService
{
    public string GetAuthorizationUrl();

    public Task<AuthCodeResponse> ProcessCallback(string code);
    public Task<User> GetUser();
    public Helix APIs { get; }
}