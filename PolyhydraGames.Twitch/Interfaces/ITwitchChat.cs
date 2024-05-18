﻿using TwitchLib.Client.Events;

namespace PolyhydraGames.Twitch.Interfaces
{
    public interface ITwitchChat
    {
        public IObservable<OnJoinedChannelArgs> OnJoinedChannel { get; }
        public IObservable<OnConnectedEventArgs> OnConnected { get; }
        public IObservable<OnMessageReceivedArgs> OnMessageReceived { get; }
        public IObservable<OnWhisperReceivedArgs> OnWhisperReceived { get; }
        public IObservable<OnNewSubscriberArgs> OnNewSubscriber { get; }
        public IObservable<OnAnnouncementArgs> OnAnnouncement { get; }
        public IObservable<OnUserJoinedArgs> OnUserJoined { get; }
        public IObservable<OnUserLeftArgs> OnUserLeft { get; }
    }
}