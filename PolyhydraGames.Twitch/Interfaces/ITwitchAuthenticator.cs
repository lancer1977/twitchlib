namespace PolyhydraGames.Twitch.Interfaces;

public interface ITwitchAuthenticator
{
    public string GetAuthorizationUrl();

    public Task<AuthCodeResponse> ProcessCallback(string code);
    public Task<User> GetUser();
}