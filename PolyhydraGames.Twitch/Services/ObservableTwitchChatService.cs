using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace PolyhydraGames.Twitch.Services
{
    /// <summary>
    /// An observable wrapper for the Twitch Chat Client
    /// </summary>
    public class ObservableTwitchChatService : ITwitchChat
    {
        public ObservableTwitchChatService(ILoggerFactory factory, TwitchClient client)
        {
            ILogger logger = factory.CreateLogger(typeof(ObservableTwitchChatService));

            _client = client;// new TwitchClient(customClient,loggerFactory: factory);
            OnAnnouncement = Observable.FromEventPattern<OnAnnouncementArgs>(_client, nameof(TwitchClient.OnAnnouncement))
                .Do(x => logger.LogTrace(nameof(OnAnnouncement)))
                .Select(x => x.EventArgs)
                .Where(x => x != null);

            _client.OnConnectionError += async (sender, args) =>
            {
                logger.LogInformation(args.Error.Message + " " + args.BotUsername);
                await Task.CompletedTask;
            };


            OnConnected = Observable.FromEventPattern<OnConnectedArgs>(_client, nameof(TwitchClient.OnConnected))
                .Do(x => logger.LogInformation(nameof(OnConnected)))
                .Select(x => x.EventArgs)
                .Where(x => x != null);

            OnJoinedChannel = Observable.FromEventPattern<OnJoinedChannelArgs>(_client, nameof(TwitchClient.OnJoinedChannel))
                .Do(x => logger.LogInformation(nameof(OnJoinedChannel)))
                .Select(x => x.EventArgs)
                .Where(x => x != null);

            OnMessageReceived = Observable.FromEventPattern<OnMessageReceivedArgs>(_client, nameof(TwitchClient.OnMessageReceived))
                .Select(x => x.EventArgs)
                .Do(x =>
                {
                    logger.LogTrace(nameof(OnMessageReceived) + x.ChatMessage.RoomId + ": " + x.ChatMessage.Username);
                })
                .Where(x => x != null);

            OnWhisperReceived = Observable.FromEventPattern<OnWhisperReceivedArgs>(_client, nameof(TwitchClient.OnWhisperReceived))
                .Select(x => x.EventArgs)
                .Do(x => logger.LogTrace(nameof(OnWhisperReceived) + ":" + x.WhisperMessage.ToConsoleFormat()))
                .Where(x => x != null);

            OnNewSubscriber = Observable.FromEventPattern<OnNewSubscriberArgs>(_client, nameof(TwitchClient.OnNewSubscriber))
                .Do(x => logger.LogTrace(nameof(OnNewSubscriber)))
                .Select(x => x.EventArgs)
                .Where(x => x != null);

            OnUserJoined = Observable.FromEventPattern<OnUserJoinedArgs>(_client, nameof(TwitchClient.OnUserJoined))
                .Do(x => logger.LogInformation(nameof(OnUserJoined)))
                .Select(x => x.EventArgs)
                .Where(x => x != null);

            OnUserLeft = Observable.FromEventPattern<OnUserLeftArgs>(_client, nameof(TwitchClient.OnUserLeft))
                .Do(x => logger.LogInformation(nameof(OnUserLeft)))
                .Select(x => x.EventArgs)
                .Where(x => x != null);

            OnRaided = Observable.FromEventPattern<OnRaidNotificationArgs>(_client, nameof(TwitchClient.OnRaidNotification))
                .Do(x => logger.LogInformation(nameof(OnRaided)))
                .Select(x => x.EventArgs)
                .Where(x => x != null);

            //OnEmoteOnly = TwitchExtensions.FromEventPattern<TwitchLib.Client.Events.OnEmoteOnlyArgs>(
            //        handler => _client.OnEmoteOnly += handler,
            //        handler => _client.OnEmoteOnly -= handler
            //    )
            //    .Do(x => logger.LogInformation(nameof(OnUserLeft)))
            //    .Where(x => x != null);

            //OnVIPsReceived = TwitchExtensions.FromEventPattern<OnVIPsReceivedArgs>(
            //        handler => _client.OnVIPsReceived += handler,
            //        handler => _client.OnVIPsReceived -= handler
            //    )
            //    .Do(x => logger.LogInformation(nameof(OnUserLeft)))
            //    .Where(x => x != null);

        }

        public void JoinChannel(string channelName) => _client.JoinChannel(channelName, true);

        public bool Connect() => _client.Connect();
        public void LeaveChannel(string channelName) => _client.LeaveChannel(channelName);
        public void SendMessage(string channel, string message) => _client.SendMessage(channel, message);

        public void Initialize(ConnectionCredentials connectionCredentials) => _client.Initialize(connectionCredentials);

        private readonly TwitchClient _client;
        public IObservable<OnJoinedChannelArgs> OnJoinedChannel { get; private init; }
        /// <summary>
        /// On Client Connect
        /// </summary>
        public IObservable<OnConnectedArgs> OnConnected { get; private init; }
        public IObservable<OnMessageReceivedArgs> OnMessageReceived { get; private init; }
        public IObservable<OnWhisperReceivedArgs> OnWhisperReceived { get; private init; }
        public IObservable<OnNewSubscriberArgs> OnNewSubscriber { get; private init; }
        public IObservable<OnAnnouncementArgs> OnAnnouncement { get; private init; }
        public IObservable<OnUserJoinedArgs> OnUserJoined { get; private init; }
        public IObservable<OnUserLeftArgs> OnUserLeft { get; private init; }
        public IObservable<OnRaidNotificationArgs> OnRaided { get; private init; }
        //public IObservable<OnEmoteOnlyArgs> OnEmoteOnly { get; private init; }
        //public IObservable<OnVIPsReceivedArgs> OnVIPsReceived { get; private init; }




    }
}