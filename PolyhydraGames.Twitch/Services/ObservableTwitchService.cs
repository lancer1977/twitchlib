using System.Reactive.Linq;
using System.Reactive.Subjects;
using OAuth.Core;
using OAuth.Core.Bot;
using OAuth.Core.Services;
using PolyhydraGames.Streaming.Interfaces;
using PolyhydraGames.Twitch.Extensions;

namespace PolyhydraGames.Twitch.Services;
public class ObservableTwitchService : QueueService<ObservableTwitchService>, IApiService, ICompositeDisposable
{
    private ObservableTwitchChatService _chat;
    private ObservableTwitchPubSubService _pubSub;
    private readonly TwitchAPI _api;
    private readonly TwitchApiConfig _config;
    private readonly IChannelRecordsCache _recordService; 
    private readonly ILogger _logger;
    private readonly string _username;
    private readonly string _state;
    // private readonly string _channelTarget;
    private readonly Dictionary<string, JoinedChannel> _joinedChannels = new();
    private readonly Subject<Unit> _registrationCompleted = new();
    public IObservable<Unit> OnRegistrationCompleted { get; }
    protected override string Name => nameof(ObservableTwitchService);
    //This is really from restart
    public override Task OnStart() => Task.CompletedTask;
    //httppost is made to get the real key
    public Task RefreshToken() => Task.CompletedTask;
    public new ICollection<IDisposable> Disposables => base.Disposables;
    public bool Connected => ConnectedState == ConnectionState.Connected;
    public ConnectionState ConnectedState { get; set; }


    public ObservableTwitchService(TwitchAPI api,
        TwitchApiConfig apiConfig,
        ILogger<ObservableTwitchService> log,
        IChannelRecordsCache recordService,
        IStreamerService streamerService,
        ObservableTwitchChatService chat, 
        ObservableTwitchPubSubService pubSub) : base(log)
    {
        OnRegistrationCompleted = _registrationCompleted.AsObservable();

        _config = apiConfig;
        _recordService = recordService; 
        _chat = chat;
        _pubSub = pubSub;
        _api = api;
        _username = apiConfig.Username; //configuration 
        _state = apiConfig.State;

        _recordService.TwitchChannelAdded.Select(async x=> await JoinChannel(x).ConfigureAwait(false))
            .Subscribe();

        _chat.OnConnected.Select(async x =>
        {
            var channels = await streamerService.Items();
            foreach (var channel in channels)
            {
                await JoinChannel(channel);
            }
        }).Subscribe()
            .AppendToDisposable(this);


        _chat.OnJoinedChannel
                .Subscribe(x=> _chat.SendMessage(x.Channel, $"Howdy {x.Channel}, hows it going? {x.BotUsername}"))
                .AppendToDisposable(this);
        
        //Updates user details
        _chat.OnMessageReceived
            .Subscribe(x => _recordService.Update(x.ChatMessage.ToUpdate()))
            .AppendToDisposable(this);

        //Processes requests
        _chat.OnMessageReceived
            .Where(x => x.ChatMessage.Message.StartsWith('!'))
            .Select(TwitchExtensions.ToRequest)
            .Subscribe(Queue)
            .AppendToDisposable(this);

        _chat.OnWhisperReceived
            .Where(x => x.WhisperMessage.Message.StartsWith('!'))
            .Select(TwitchExtensions.ToRequest)
            .Subscribe(Queue)
            .AppendToDisposable(this);

        _chat.OnUserJoined
            .Subscribe(x => _recordService.Update(x.ToJoinUpdate()))
            .AppendToDisposable(this);

        _chat.OnUserLeft
            .Subscribe(x => _recordService.Update(x.ToLeftUpdate()))
            .AppendToDisposable(this);
    }

    /// <summary>
    /// - If in list, (maybe name changed), remove then readd
    /// - Add to local list of JoinedChannels
    /// - Register with Client for messages from channel.
    /// </summary>
    /// <param name="channelName"></param>
    private async Task JoinChannel(IStreamer streamer)
    {
        var channelName = streamer.TwitchChannelName;
        var id = streamer.TwitchChannelId;
        var auth = streamer.TwitchOAuth;
        Log.LogTrace($"Twitch: Request Joining {channelName}");
        if (string.IsNullOrEmpty(channelName))
        {
            return;
        }
        if (!Connected) return;

        _joinedChannels.Remove(channelName);
        _joinedChannels.Add(channelName, new JoinedChannel(channelName));
        _chat.JoinChannel(channelName);

        try
        {
            if (streamer.TwitchChannelId == null)
            {
                id = _api.Helix.Users.GetUsersAsync(logins: [channelName]).Result.Users[0].Id.ToInt();
            }

            if (!string.IsNullOrEmpty(auth))
            {
                _pubSub.RegisterChannel(id, auth);
            }
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error acquiring the user id or registering pubsub");
        }
        Log.LogInformation($"Twitch channel {channelName} Started");
    }

    private void LeaveChannel(string channelName)
    {
        Log.LogTrace($"Twitch: Leaving {channelName}");
        _joinedChannels.Remove(channelName);
        _chat.LeaveChannel(channelName);
    }

    public override async Task ProcessResult(CommandResult result)
    {
        //If the backend didn't return a response to display, do not try to display anything. This occurs if the message was not something we've decided to handle, ie a command that doesn't exist.
        if (string.IsNullOrEmpty(result.Message)) return;
        Log.LogTrace($"Twitch: {(result.IsWhisper ? "Whispering " : "Messaging")} {result.Target}");

        if (result.IsWhisper)
        {
            //_api.Helix.EventSub.
            //TODO: Fix this
            //await _api.Helix.Chat.  SendWhisperAsync(_config.Username, result.TargetId, result.Message, false, _config.AccessToken);
        }
        else
        {
            _chat.SendMessage(result.Target, result.Message);
        }
    }

    public string GetAuthorizationUrl()
    {
        var code = _api.Auth.GetAuthorizationCodeUrl(_config.RedirectUrl, _config.Scopes, state: _state, clientId: _config.ClientId);
        code = code.InjectScope("whispers:read+whispers:edit+");
        return code;
    }

    /// <summary>
    /// Callback from the website for authentication
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task Callback(string code)
    {
        try
        {
            var result = await _api.Auth.GetAccessTokenFromCodeAsync(code, _config.Secret, _config.RedirectUrl, _config.ClientId);
            _api.Settings.AccessToken = result.AccessToken;
            _api.Settings.ClientId = _config.ClientId;
            if (result.AccessToken == null) return;
            _chat.Initialize(new ConnectionCredentials(_username, result.AccessToken));

            _pubSub.Connect();

            ConnectedState = _chat.Connect() ? ConnectionState.Connected : ConnectionState.CriticalFailure;
            Log.LogInformation("Twitch: Authentication Completed.");
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Twitch Callback Failed");
        }
    }
}
