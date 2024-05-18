namespace PolyhydraGames.Twitch.Models
{
    public class TwitchMessageExpanded: TwitchMessage
    { 
        public string BadgeInfo { get; set; }
        public Dictionary<string, string> Badges { get; set; }
        public string ClientNonce { get; set; }
        public string Color { get; set; } 
        public string Emotes { get; set; }
        public int FirstMsg { get; set; }
        public string Flags { get; set; }
        public string Id { get; set; }
        public int Mod { get; set; }
        public int ReturningChatter { get; set; }
        public string RoomId { get; set; }
        public int Subscriber { get; set; }
        public long TmiSentTs { get; set; }
        public int Turbo { get; set; }
        public string UserId { get; set; }
        public string UserType { get; set; }

    }
}