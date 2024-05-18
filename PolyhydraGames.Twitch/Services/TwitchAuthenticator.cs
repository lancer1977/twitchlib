namespace PolyhydraGames.Twitch.Services;

public class TwitchAuthenticator : ITwitchAuthenticator
{
    private readonly TwitchAPI _api;
    private readonly TwitchApiConfig _apiconfig;
    private readonly ILogger<TwitchAuthenticator> _log;

    public TwitchAuthenticator(TwitchApiConfig apiconfig, ILogger<TwitchAuthenticator> log)
    { 
        _api = new TwitchAPI(settings:apiconfig);
        _apiconfig = apiconfig;
        _log = log;
    }

    public string GetAuthorizationUrl()
    {
        //state: _state,
        var code = _api.Auth.GetAuthorizationCodeUrl(_apiconfig.UserRedirectUrl, _apiconfig.Scopes,  clientId: _apiconfig.ClientId);
        code = code.InjectScope("whispers:read+whispers:edit+channel:read:redemptions+");
        return code;
    }

    public async Task<AuthCodeResponse> ProcessCallback(string code)
    {
        try
        {
            var response =  await _api.Auth.GetAccessTokenFromCodeAsync(code, _apiconfig.Secret, _apiconfig.UserRedirectUrl, _apiconfig.ClientId);
            _api.Settings.AccessToken = response.AccessToken;
            _api.Settings.ClientId = _apiconfig.ClientId;
            return response;
        }
        catch (Exception ex)
        {
           return _log.LogAndThrow< AuthCodeResponse>(ex);
        }
    }

    public async Task<User> GetUser()
    {
        var result = await _api.Helix.Users.GetUsersAsync();
        return result.Users[0];
    }
}