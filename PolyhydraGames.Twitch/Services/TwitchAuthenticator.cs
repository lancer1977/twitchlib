using TwitchLib.Api.Helix;

namespace PolyhydraGames.Twitch.Services;

public class StreamerTwitchService : IStreamerTwitchService
{
    private readonly TwitchAPI _api;
    private readonly TwitchApiConfig _apiconfig;
    private readonly ILogger<StreamerTwitchService> _log;

    public StreamerTwitchService(TwitchApiConfig apiconfig, ILogger<StreamerTwitchService> log)
    { 
        _api = new TwitchAPI(settings:apiconfig);
        _apiconfig = apiconfig;
        _log = log;
    }

    /// <summary>
    /// Generates the authorization URL for the user to log in, was issues with missing whisper scopes so manually added them
    /// </summary>
    /// <returns></returns>
    public string GetAuthorizationUrl()
    {
        //state: _state,
        var code = _api.Auth.GetAuthorizationCodeUrl(_apiconfig.UserRedirectUrl, _apiconfig.Scopes,  clientId: _apiconfig.ClientId);
        code = code.InjectScope("whispers:read+whispers:edit+channel:read:redemptions+");
        return code;
    }

    /// <summary>
    /// Callback endpoint for the user to return to after logging in
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets the user information for the authenticated user
    /// </summary>
    /// <returns></returns>
    public async Task<User> GetUser()
    {
        
        var result = await _api.Helix.Users.GetUsersAsync();
        return result.Users[0];
    }

    public Helix APIs => _api.Helix;
}