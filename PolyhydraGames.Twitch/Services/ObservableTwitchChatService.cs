using System.Reactive.Linq;
using PolyhydraGames.Twitch.Extensions;
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
            OnAnnouncement = TwitchExtensions.FromEventPattern<OnAnnouncementArgs>(
                    handler => _client.OnAnnouncement += handler,
                    handler => _client.OnAnnouncement -= handler
                ).Do(x => logger.LogTrace(nameof(OnAnnouncement)))
                .Select(x => x)
                .Where(x => x != null);
            _client.OnConnectionError += async (sender, args) =>
            {
                logger.LogInformation(args.Error.Message + " " + args.BotUsername);
                await Task.CompletedTask;
            };

            _client.OnConnected += (sender, args) =>
            {
                logger.LogInformation(nameof(OnConnected) + " " +  args.BotUsername);
                return Task.CompletedTask;
            };
            OnConnected = TwitchExtensions.FromEventPattern<OnConnectedEventArgs>(
                    handler => _client.OnConnected += handler,
                    handler => _client.OnConnected -= handler
                )
                .Do(x => logger.LogInformation(nameof(OnConnected)))
                .Where(x => x != null);

            OnJoinedChannel = TwitchExtensions.FromEventPattern<OnJoinedChannelArgs>(
                    handler => _client.OnJoinedChannel += handler,
                    handler => _client.OnJoinedChannel -= handler
                )
                .Do(x => logger.LogInformation(nameof(OnJoinedChannel)))
                .Where(x => x != null);

            OnMessageReceived = TwitchExtensions.FromEventPattern<OnMessageReceivedArgs>
                (
                    handler => _client.OnMessageReceived += handler,
                    handler => _client.OnMessageReceived -= handler
                )
                .Do(x =>
                {
                    logger.LogTrace(nameof(OnMessageReceived) + x.ChatMessage.RoomId + ": " + x.ChatMessage.Username);
                })
                .Where(x => x != null);

            OnWhisperReceived = TwitchExtensions
                .FromEventPattern<OnWhisperReceivedArgs>(
                    handler => _client.OnWhisperReceived += handler,
                    handler => _client.OnWhisperReceived -= handler
                )
                .Do(x => logger.LogTrace(nameof(OnWhisperReceived) + ":" + x.WhisperMessage.ToConsoleFormat()))
                .Where(x => x != null);

            OnNewSubscriber = TwitchExtensions.FromEventPattern<OnNewSubscriberArgs>(
                    handler => _client.OnNewSubscriber += handler,
                    handler => _client.OnNewSubscriber -= handler
                )
                .Do(x => logger.LogTrace(nameof(OnNewSubscriber)))
                .Where(x => x != null);

            OnUserJoined = TwitchExtensions.FromEventPattern<OnUserJoinedArgs>(
                    handler => _client.OnUserJoined += handler,
                    handler => _client.OnUserJoined -= handler
                )
                .Do(x => logger.LogInformation(nameof(OnUserJoined)))
                .Where(x => x != null);

            OnUserLeft = TwitchExtensions.FromEventPattern<OnUserLeftArgs>(
                    handler => _client.OnUserLeft += handler,
                    handler => _client.OnUserLeft -= handler
                )
                .Do(x => logger.LogInformation(nameof(OnUserLeft)))
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

        public Task JoinChannel(string channelName) => _client.JoinChannelAsync(channelName, true);

        public Task<bool> ConnectAsync() => _client.ConnectAsync(); 
        public Task LeaveChannel(string channelName) => _client.LeaveChannelAsync(channelName);
        public Task SendMessageAsync(string channel, string message) => _client.SendMessageAsync(channel, message);

        public void Initialize(ConnectionCredentials connectionCredentials) => _client.Initialize(connectionCredentials);

        private readonly TwitchClient _client;
        public IObservable<OnJoinedChannelArgs> OnJoinedChannel { get; private init; }
        /// <summary>
        /// On Client Connect
        /// </summary>
        public IObservable<OnConnectedEventArgs> OnConnected { get; private init; }
        public IObservable<OnMessageReceivedArgs> OnMessageReceived { get; private init; }
        public IObservable<OnWhisperReceivedArgs> OnWhisperReceived { get; private init; }
        public IObservable<OnNewSubscriberArgs> OnNewSubscriber { get; private init; }
        public IObservable<OnAnnouncementArgs> OnAnnouncement { get; private init; }
        public IObservable<OnUserJoinedArgs> OnUserJoined { get; private init; }
        public IObservable<OnUserLeftArgs> OnUserLeft { get; private init; }
        //public IObservable<OnEmoteOnlyArgs> OnEmoteOnly { get; private init; }
        //public IObservable<OnVIPsReceivedArgs> OnVIPsReceived { get; private init; }


        

    }
}