using System.Diagnostics;
using System.Text.Json.Serialization;
using TwitchLib.PubSub.Events;

namespace PolyhydraGames.Twitch.Models
{
    public class ChannelPointsRedemption
    {
        [JsonPropertyName("redemptionName")]
        public string RedemptionName { get; set; }
        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("userId")] 
        public string UserId { get; set; }
        [JsonPropertyName("channelId")] 
        public string ChannelId { get; set; }
        [JsonPropertyName("channelName")]
        public string ChannelName { get; set; }
        [JsonPropertyName("userInput")]
        public string UserInput { get; set; }


    }
}