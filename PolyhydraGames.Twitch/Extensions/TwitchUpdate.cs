namespace PolyhydraGames.Twitch.Extensions;

public class TwitchUpdate : ITwitchUpdate
{ 
    public int? Id { get; set; }
    public string Name { get; set; }
    public string ChannelTarget { get; set; }
    public bool? IsFollower { get; set; }
    public bool? IsSubscriber { get; set; }
    public bool? IsModerator { get; set; }
    public bool IsOnline { get; set; }
    public int? Bits { get; set; }
}