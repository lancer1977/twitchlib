using TwitchLib.PubSub.Events;

namespace PolyhydraGames.Twitch.Interfaces
{
    public interface ITwitchPubSub
    {
        public IObservable<OnCommercialArgs> OnCommercial { get; }

        public IObservable<OnChannelPointsRewardRedeemedArgs> OnChannelPointsRewardRedeemed { get; }
        public IObservable<OnStreamDownArgs> OnStreamDown { get; }

        public IObservable<OnStreamUpArgs> OnStreamUp { get; }

        public IObservable<OnListenResponseArgs> OnListenResponse { get; }
        public IObservable<OnRaidUpdateV2Args> OnRaidStart { get; }

        public IObservable<OnFollowArgs> OnFollow { get; }

        public IObservable<EventArgs> OnConnected { get; }

    }
}