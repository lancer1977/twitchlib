//using OAuth.Core.Bot;
//using OAuth.Core.Services;
//using System.Reactive.Linq;
//using System.Reactive.Subjects;
//using OAuth.Core;
//using PolyhydraGames.Streaming.Interfaces;
//using PolyhydraGames.Twitch.Extensions;

//namespace PolyhydraGames.Twitch.Services;

//public class TwitchService_old : QueueService<TwitchService_old>, IApiService, ICompositeDisposable
//{
//    private ObservableTwitchChatService _chat;
//    private ObservableTwitchPubSubService _pubSub;
//    private readonly TwitchAPI _api;

//    private readonly TwitchApiConfig _config;
//    private readonly IChannelRecordsCache _recordService;

//    private readonly ILogger _logger;
//    private readonly string _username;
//    private readonly string _state;
//    // private readonly string _channelTarget;
//    private readonly Dictionary<string, JoinedChannel> _joinedChannels = new();

//    private readonly Subject<Unit> _registrationCompleted = new();
//    public IObservable<Unit> OnRegistrationCompleted { get; }

//    public bool Connected => ConnectedState == ConnectionState.Connected;
//    public ConnectionState ConnectedState { get; set; }
//    /// <summary>
//    /// When my app joins a channel
//    /// </summary>



//    public TwitchService_old(TwitchAPI api,
//        TwitchApiConfig apiConfig,
//        ILogger<TwitchService_old> log,
//        IChannelRecordsCache recordService,
//        IStreamerService streamerService,
//        ObservableTwitchChatService chat, ObservableTwitchPubSubService pubSub) : base(log)
//    {
//        OnRegistrationCompleted = _registrationCompleted.AsObservable();
//        _state = apiConfig.State;
//        _config = apiConfig;
//        _recordService = recordService;
//        _chat = chat;
//        _pubSub = pubSub;
//        _api = api;
//        _username = apiConfig.Username; //configuration 
//        _state = apiConfig.State;

//        _recordService.TwitchChannelAdded
//            .Where(x => !string.IsNullOrEmpty(x))
//            .Subscribe(JoinChannel);

//        _chat.OnConnected.Select(async x =>
//        {
//            var channels = await streamerService.Items();
//            foreach (var channel in channels)
//            {
//                JoinChannel(channel.TwitchChannelName);
//            }
//        }).Subscribe()
//            .AppendToDisposable(this);


//        _chat.OnJoinedChannel.Select(x => Unit.Default).Subscribe()
//            .AppendToDisposable(this);


//        //Updates user details
//        _chat.OnMessageReceived.Subscribe(x =>
//        {
//            _recordService.Update(x.ChatMessage.ToUpdate());
//        }).AppendToDisposable(this);

//        //Processes requests
//        _chat.OnMessageReceived
//            .Where(x => x.ChatMessage.Message.StartsWith("!"))
//            .Select(x => new CommandRequest(x.ChatMessage.Username, x.ChatMessage.Message, x.ChatMessage.Channel, x.ChatMessage.IsNonPleb()))
//            .Subscribe(Queue)
//            .AppendToDisposable(this);

//        _chat.OnWhisperReceived

//            .Where(x => x.WhisperMessage.Message.StartsWith("!"))
//            .Select(x => new CommandRequest(x.WhisperMessage.Username, x.WhisperMessage.Message, "", false))
//            .Subscribe(Queue)
//            .AppendToDisposable(this);

//        _chat.OnUserJoined.Subscribe(x => { _recordService.Update(x.ToJoinUpdate()); }).AppendToDisposable(this);
//        _chat.OnUserLeft.Subscribe(x => { _recordService.Update(x.ToLeftUpdate()); }).AppendToDisposable(this);
//    }

//    /// <summary>
//    /// - If in list, (maybe name changed), remove then readd
//    /// - Add to local list of JoinedChannels
//    /// - Register with Client for messages from channel.
//    /// </summary>
//    /// <param name="channelName"></param>
//    private void JoinChannel(string channelName)
//    {
//        Log.LogTrace($"Twitch: Request Joining {channelName}");
//        if (!Connected) return;

//        _joinedChannels.Remove(channelName);
//        _joinedChannels.Add(channelName, new JoinedChannel(channelName));
//        _chat.JoinChannel(channelName);
//        try
//        {
//            var id = _api.Helix.Users.GetUsersAsync(logins: new List<string> { channelName }).Result.Users[0].Id;
//            _pubSub.RegisterChannel(id);

//        }
//        catch (Exception ex)
//        {
//            Log.LogError(ex, "Error aquiring the user id or registering pubsub");
//        }

//        Log.LogInformation($"Twitch channel {channelName} Started");
//    }

//    private void LeaveChannel(string channelName)
//    {
//        Log.LogTrace($"Twitch: Leaving {channelName}");
//        _joinedChannels.Remove(channelName);
//        _chat.LeaveChannel(channelName);
//    }
//    protected override string Name => nameof(TwitchService_old);

//    public override async Task ProcessResult(CommandResult result)
//    {
//        //If the backend didn't return a response to display, do not try to display anything. This occurs if the message was not something we've decided to handle, ie a command that doesn't exist.
//        if (string.IsNullOrEmpty(result.Message)) return;
//        Log.LogTrace($"Twitch: {(result.IsWhisper ? "Whispering " : "Messaging")} {result.Target}");

//        if (result.IsWhisper)
//        {
//            await _api.Helix.Whispers.SendWhisperAsync(_config.Username, result.TargetId,
//                result.Message, false, _config.AccessToken);
//        }
//        else
//        {

//            _chat.SendMessage(result.Target, result.Message);
//        }
//    }

//    //This is really from restart
//    public override Task OnStart() => Task.CompletedTask;


//    //TODO: Consider using TwitchAuthenticator class.
//    public string GetAuthorizationUrl()
//    {
//        var code = _api.Auth.GetAuthorizationCodeUrl(_config.RedirectUrl, _config.Scopes, state: _state, clientId: _config.ClientId);
//        code = code.InjectScope("whispers:read+whispers:edit+");
//        return code;
//    }

//    //httppost is made to get the real key
//    public Task RefreshToken() => Task.CompletedTask;


//    public async Task Callback(string code)
//    {
//        try
//        {
//            var result = await _api.Auth.GetAccessTokenFromCodeAsync(code, _config.Secret, _config.RedirectUrl, _config.ClientId);

//            if (result.AccessToken == null) return;
//            _chat.Initialize(new ConnectionCredentials(_username, result.AccessToken));

//            await _pubSub.ConnectAsync();
//            ConnectedState = await _chat.ConnectAsync() ? ConnectionState.Connected : ConnectionState.CriticalFailure;
//            Log.LogInformation("Twitch: Authentication Completed.");
//        }
//        catch (Exception ex)
//        {
//            Log.LogError(ex, "Twitch Callback Failed");
//        }
//    }

//    //public void OnLogAction(OnLogArgs logAction)
//    //{
//    //    try
//    //    {
//    //        Log.LogTrace(logAction.Data);
//    //        var data = logAction.Data;

//    //        if (data.Contains("authentication failed"))
//    //        {
//    //            ConnectedState = ConnectionState.CriticalFailure;
//    //        }
//    //        else
//    //        {
//    //            var message = data.ParseMessage();
//    //            Task.Run(async () => await _recordService.Update(message));
//    //        }
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        Log.LogError(ex, "OnLogAction");
//    //    } 
//    //}


//    public new ICollection<IDisposable> Disposables => base.Disposables;
//}