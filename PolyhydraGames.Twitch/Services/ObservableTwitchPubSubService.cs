﻿
using System.Reactive.Linq;
using PolyhydraGames.Twitch.Extensions;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace PolyhydraGames.Twitch.Services
{
    public class ObservableTwitchPubSubService : ITwitchPubSub
    {
        private readonly TwitchPubSub _client; 

        public ObservableTwitchPubSubService(TwitchPubSub client, ILogger<ObservableTwitchPubSubService> log)
        {
            _client = client; 

            OnChannelPointsRewardRedeemed = Observable.FromEventPattern<OnChannelPointsRewardRedeemedArgs>(_client, nameof(TwitchPubSub.OnChannelPointsRewardRedeemed))
                .Do(x =>
                {
                    log.LogInformation(nameof(TwitchPubSub.OnChannelPointsRewardRedeemed));
                })
                .Select(x => x.EventArgs)
                .Where(x => x != null);

            OnListenResponse = TwitchExtensions.FromEventPattern<OnListenResponseArgs>(_client, nameof(TwitchPubSub.OnListenResponse), log);
            OnStreamUp = TwitchExtensions.FromEventPattern<OnStreamUpArgs>(_client, nameof(TwitchPubSub.OnStreamUp), log);
            OnStreamDown = TwitchExtensions.FromEventPattern<OnStreamDownArgs>(_client, nameof(TwitchPubSub.OnStreamDown), log);
            OnCommercial = TwitchExtensions.FromEventPattern<OnCommercialArgs>(_client, nameof(TwitchPubSub.OnCommercial), log);
            OnPubSubServiceConnected = TwitchExtensions.FromEventPattern<EventArgs>(_client, nameof(TwitchPubSub.OnPubSubServiceConnected), log);

            OnPubSubServiceConnected.Subscribe(x =>
            {
                log.LogInformation("PubSub Connected");
            });
            OnListenResponse.Subscribe(x =>
            {
                if (x.Successful)
                    log.LogInformation($"{x.ChannelId} {x.ChannelId} {x.Topic}");
                else
                    log.LogError($"{x.ChannelId} {x.ChannelId} {x.Topic} {x.Response.Error}");

            });

            //OnChannelPointsRewardRedeemed.Subscribe(x =>
            //{
            //    Debug.WriteLine($"{x.ChannelId} {x.RewardRedeemed.Redemption.Reward.Cost} {x.RewardRedeemed.Redemption.Reward.Title}  ");  
            //});
        }

        public IObservable<OnCommercialArgs> OnCommercial { get; }

        public IObservable<OnChannelPointsRewardRedeemedArgs> OnChannelPointsRewardRedeemed { get; }
        public IObservable<OnStreamDownArgs> OnStreamDown { get; }

        public IObservable<OnStreamUpArgs> OnStreamUp { get; }

        public IObservable<OnListenResponseArgs> OnListenResponse { get; }

        public IObservable<EventArgs> OnPubSubServiceConnected { get; }

        public void Connect() => _client.Connect();

        /// <summary>
        /// This has to be called before any other methods will be listened on.
        /// </summary>
        /// <param name="channelId"></param>
        public void RegisterChannel(int? channelId, string authcode)
        {
            _client.ListenToSubscriptions(channelId.ToString());
            _client.ListenToChannelPoints(channelId.ToString());
            _client.SendTopics(authcode);
        }
    }
}