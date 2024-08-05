using TwitchLib.Api.Helix;

namespace PolyhydraGames.Twitch.Services;

public class StreamerTwitchService : IStreamerTwitchService
{
    private readonly TwitchAPI _api; 
    private readonly ILogger<StreamerTwitchService> _log;
    private readonly TwitchApiConfig _settings;
    public Dictionary<int, ExtendedViewerDetails> ViewerCache { get; } = new();
    public StreamerTwitchService(TwitchApiConfig apiconfig, ILogger<StreamerTwitchService> log)
    {
    
       _settings = apiconfig.GetCopy();
        _api = new TwitchAPI(settings: _settings); 
        _log = log;
    }

    /// <summary>
    /// Generates the authorization URL for the user to log in, was issues with missing whisper scopes so manually added them
    /// </summary>
    /// <returns></returns>
    public string GetAuthorizationUrl()
    {
        //state: _state,
        var code = _api.Auth.GetAuthorizationCodeUrl(_settings.UserRedirectUrl, _settings.Scopes,  clientId: _settings.ClientId);
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
            var response =  await _api.Auth.GetAccessTokenFromCodeAsync(code, _settings.Secret, _settings.UserRedirectUrl, _settings.ClientId);
            _api.Settings.AccessToken = response.AccessToken;
            _api.Settings.ClientId = _settings.ClientId;
            _settings.AccessToken = response.AccessToken;
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

public class ExtendedViewerDetails
{
    private readonly IViewer _viewer; 
    public ExtendedViewerDetails(IViewer viewer)
    {
        _viewer = viewer;
    }
}