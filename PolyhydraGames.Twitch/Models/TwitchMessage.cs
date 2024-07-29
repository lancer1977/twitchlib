namespace PolyhydraGames.Twitch.Models;


public class TwitchMessage : IStreamMessage
{
    public StreamingPlatform Platform { get; set; }
    public MessageType MessageType { get; set; }
    public string ChannelName { get; set; }

    public string Message { get; set; }
    public string UserName { get; set; }
}