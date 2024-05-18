using OAuth.Core.Services;

namespace PolyhydraGames.Twitch.Configs;


public class TwitchApiConfig : ITwitchClientConfig, INotifyPropertyChanged
{
    private List<AuthScopes> _scopes;
    private string _userRedirectUrl;
    private string _authorizationEndpoint;
    private string _tokenEndpoint;
    private string _redirectUrl;
    private string _accessToken;
    private string _secret;
    private string _clientId;
    private bool _skipDynamicScopeValidation;
    private bool _skipAutoServerTokenGeneration;

    public TwitchApiConfig(IConfiguration config)
    {
        Scopes =
        [
            AuthScopes.OpenId,
            
            AuthScopes.Channel_Commercial,
            AuthScopes.Channel_Feed_Edit,
            AuthScopes.Channel_Feed_Read,
            AuthScopes.Channel_Read,
            AuthScopes.Channel_Stream,
            AuthScopes.Channel_Subscriptions,
            AuthScopes.Helix_Channel_Manage_Redemptions,
            AuthScopes.Chat_Read,
            AuthScopes.Chat_Edit,
            AuthScopes.Helix_User_Manage_Whispers,
            AuthScopes.Helix_Channel_Read_Charity,
            AuthScopes.Helix_Channel_Read_Editors,
            AuthScopes.Helix_Channel_Read_Goals,
            AuthScopes.Helix_Channel_Read_Hype_Train,
            AuthScopes.Helix_Channel_Read_Polls,
            AuthScopes.Helix_Channel_Read_Predictions,
            AuthScopes.Helix_Channel_Read_Redemptions,
            AuthScopes.Helix_Channel_Read_Stream_Key,
            AuthScopes.Helix_Channel_Read_Subscriptions,
            AuthScopes.Helix_Channel_Read_VIPs
        ];
        //Scopes = string.Join(" ", scopes);
        AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize";
        TokenEndpoint = "https://id.twitch.tv/oauth2/token"; //	Token URL 
        RedirectUrl = config["Twitch:Redirect"];
        Secret = config["Twitch:Secret"];
        ClientId = config["Twitch:ClientId"];
        Username = config["Twitch:BotName"];
        State = config["Twitch:State"];
        UserRedirectUrl = config["Twitch:UserRedirect"];

    }

    public List<AuthScopes> Scopes
    {
        get => _scopes;
        set
        {
            if (Equals(value, _scopes)) return;
            _scopes = value;
            OnPropertyChanged();
        }
    }

    public string Username { get; }
    public string State { get; }

    public string UserRedirectUrl
    {
        get => _userRedirectUrl;
        set
        {
            if (value == _userRedirectUrl) return;
            _userRedirectUrl = value;
            OnPropertyChanged();
        }
    }

    public string AuthorizationEndpoint
    {
        get => _authorizationEndpoint;
        set
        {
            if (value == _authorizationEndpoint) return;
            _authorizationEndpoint = value;
            OnPropertyChanged();
        }
    }

    public string TokenEndpoint
    {
        get => _tokenEndpoint;
        set
        {
            if (value == _tokenEndpoint) return;
            _tokenEndpoint = value;
            OnPropertyChanged();
        }
    }

    public string RedirectUrl
    {
        get => _redirectUrl;
        set
        {
            if (value == _redirectUrl) return;
            _redirectUrl = value;
            OnPropertyChanged();
        }
    }

    public string AccessToken
    {
        get => _accessToken;
        set
        {
            if (value == _accessToken || string.IsNullOrEmpty(value)) return;
            _accessToken = value;
            OnPropertyChanged();
        }
    }

    public string Secret
    {
        get => _secret;
        set
        {
            if (value == _secret) return;
            _secret = value;
            OnPropertyChanged();
        }
    }

    public string ClientId
    {
        get => _clientId;
        set
        {
            if (value == _clientId) return;
            _clientId = value;
            OnPropertyChanged();
        }
    }

    public bool SkipDynamicScopeValidation
    {
        get => _skipDynamicScopeValidation;
        set
        {
            if (value == _skipDynamicScopeValidation) return;
            _skipDynamicScopeValidation = value;
            OnPropertyChanged();
        }
    }

    public bool SkipAutoServerTokenGeneration
    {
        get => _skipAutoServerTokenGeneration;
        set
        {
            if (value == _skipAutoServerTokenGeneration) return;
            _skipAutoServerTokenGeneration = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    //protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    //{
    //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
    //    field = value;
    //    OnPropertyChanged(propertyName);
    //    return true;
    //}
}