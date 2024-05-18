#nullable enable
using System.Diagnostics;
using PolyhydraGames.Streaming.Interfaces;
using PolyhydraGames.Streaming.Interfaces.Enums;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using OAuth.Core.Bot;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Events;
using TwitchLib.PubSub.Events;

namespace PolyhydraGames.Twitch.Extensions;

public static class TwitchExtensions
{
    public static ChannelPointsRedemption ToModel(this OnChannelPointsRewardRedeemedArgs args)
    {
        return new ChannelPointsRedemption
        {
            UserInput = args.RewardRedeemed.Redemption.UserInput,
            UserName = args.RewardRedeemed.Redemption.User.Login,
            UserId = args.RewardRedeemed.Redemption.User.Id,
            ChannelId = args.ChannelId,
            RedemptionName = args.RewardRedeemed.Redemption.Reward.Title,
            //ChannelName = args.ChannelName
        };
    }

    public static IObservable<T> FromEventPattern<T>(object obj, string eventName, ILogger log)
    {
        Debug.WriteLine(eventName);
        return Observable.FromEventPattern<T>(obj, eventName)
            .Do(x =>
            {
                log.LogInformation(eventName);
            })
            .Select(x => x.EventArgs);
    }
    /// <summary>
    /// Wraps an event pattern into an IObservable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="obj"></param>
    /// <param name="eventName"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static IObservable<T2> FromEventPattern<T, T2>(object obj, string eventName, ILogger log)
    {
        return Observable.FromEventPattern<T, T2>(obj, eventName)
            .Do(x => log.LogInformation(eventName))
            .Select(x => x.EventArgs)
            .Where(x => x != null);
    }

    /// <summary>
    /// Returns an IObservable for an AsyncEventHandler event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="addHandler"></param>
    /// <param name="removeHandler"></param>
    /// <returns></returns>
    public static IObservable<T> FromEventPattern<T>(Action<AsyncEventHandler<T>> addHandler, Action<AsyncEventHandler<T>> removeHandler)
    {
        var sub = new Subject<T>();
        AsyncEventHandler<T> handler = async (sender, args) =>
        {
            sub.OnNext(args);
            //return Task.CompletedTask;
        };

        addHandler(handler);
        var unsubscribe = new Action(() => removeHandler(handler));
        return sub.Finally(unsubscribe);
    }

    public static ITwitchUpdate ToJoinUpdate(this OnUserJoinedArgs args)
    {
        return new TwitchUpdate()
        {
            ChannelTarget = args.Channel,
            Name = args.Username,

        };
    }

    public static ITwitchUpdate ToLeftUpdate(this OnUserLeftArgs args)
    {
        return new TwitchUpdate()
        {
            ChannelTarget = args.Channel,
            Name = args.Username
        };
    }
    public static ITwitchUpdate ToUpdate(this ChatMessage message)
    {
        var result = message.UserId.ToInt();

        return new TwitchUpdate
        {
            Bits = message.Bits,
            IsModerator = message.IsModerator,
            IsSubscriber = message.IsSubscriber,
            Name = message.Username,
            ChannelTarget = message.Channel,
            Id = result == 0 ? null : result,

        };
    }

    public static ITwitchUpdate ToUpdate(this TwitchMessage message)
    {
        return new TwitchUpdate
        {
            Name = message.UserName,
            ChannelTarget = message.ChannelName
        };
    }
    public static string ToConsoleFormat(this WhisperMessage message)
    {
        return $"*W* {message.Username}:{message.Message}";
    }

    public static bool IsNonPleb(this ChatMessage message)
    {
        return message.IsBroadcaster || message.IsModerator || message.IsSubscriber || message.IsVip;
    }

    public static string GetChannelNameRegex(this string logMessage)
    {
        Regex regex = new(@"PRIVMSG #([^ ]+) :(.+)$");
        var match = regex.Match(logMessage);

        if (!match.Success) return string.Empty;
        var name = match.Groups[1].Value;
        return name;
    }

    //public static string GetChannelName(this string logMessage)
    //{
    //    var parts = logMessage.Split(":");
    //    var messageData = parts[2].Substring(parts[2].IndexOf('#') +1); 


    //    return messageData;
    //}

    static Dictionary<string, string> ParseBadges(string badges)
    {
        if (string.IsNullOrEmpty(badges))
        {
            return new Dictionary<string, string>();
        }

        return badges.Split(',').Select(badge => badge.Split('/')).ToDictionary(badgeParts => badgeParts[0], badgeParts => badgeParts[1]);
    }

    public static MessageType ToMessageType(this string input)
    {
        input = input.Trim();
        if (input.Contains("PRIVMSG")) return MessageType.IM;
        if (input.Contains("JOIN")) return MessageType.Join;
        if (input.Contains("PART")) return MessageType.Leave;
        if (input == "PING") return MessageType.Ping;
        if (input == "PONG") return MessageType.Pong;


        return MessageType.Info;
    }

    private static TwitchMessage ParseIM(this string[] parts)
    {
        if (parts[0] != "Received") return ParseInfo(parts);
        if (string.IsNullOrWhiteSpace(parts[1])) return ParseServerJoinMessage(parts);
        var messageData = parts[2].Trim().Split(' ');
        var displayName = messageData[0].Split('!')[0];
        var channelName = messageData[2].Substring(1); // Use Substring to remove '#' more efficiently
        var messageType = messageData[1].ToMessageType();
        var twitchMessage = new TwitchMessage
        {
            UserName = displayName,

            ChannelName = channelName,
            Message = parts.Length == 4 ? parts[3] : string.Join(':', parts[3..]),
            MessageType = messageType

        };
        return twitchMessage;
    }

    private static TwitchMessage ParseServerJoinMessage(string[] parts)
    {
        var twitchMessage = new TwitchMessage
        {
            ChannelName = parts[2].Split(" ")[2],
            Message = parts[3],
            MessageType = MessageType.Info
        };
        return twitchMessage;
    }

    private static TwitchMessageExpanded ParseIMExpanded(this string[] parts)
    {
        var messageData = parts[2].Trim().Split(' ');
        var displayName = messageData[0].Split('!')[0];
        var channelName = messageData[2].Substring(1); // Use Substring to remove '#' more efficiently
        var messageType = messageData[1].ToMessageType();
        var twitchMessage = new TwitchMessageExpanded()
        {
            UserName = displayName,
            ChannelName = channelName,
            Message = parts.Length == 4 ? parts[3] : string.Join(':', parts[3..]),
            MessageType = messageType

        };
        var tagsPart = parts[1].Substring(2);
        foreach (var tag in tagsPart.Split(';'))
        {
            var keyValue = tag.Split('=');
            var key = keyValue[0];
            var value = keyValue[1];

            // Assign values to corresponding properties

            switch (key)
            {
                case "mod":
                    twitchMessage.Mod = int.Parse(value);
                    break;
                case "room-id":
                    twitchMessage.RoomId = value;
                    break;
                case "subscriber":
                    twitchMessage.Subscriber = int.Parse(value);
                    break;
                case "user-id":
                    twitchMessage.UserId = value;
                    break;
                case "badge-info":
                    twitchMessage.BadgeInfo = value;
                    break;
                case "badges":
                    twitchMessage.Badges = ParseBadges(value);
                    break;
                case "client-nonce":
                    twitchMessage.ClientNonce = value;
                    break;
                case "color":
                    twitchMessage.Color = value;
                    break;
                case "display-name":
                    twitchMessage.UserName = value;
                    break;
                case "emotes":
                    twitchMessage.Emotes = value;
                    break;
                case "first-msg":
                    twitchMessage.FirstMsg = int.Parse(value);
                    break;
                case "flags":
                    twitchMessage.Flags = value;
                    break;
                case "id":
                    twitchMessage.Id = value;
                    break;

                case "returning-chatter":
                    twitchMessage.ReturningChatter = int.Parse(value);
                    break;

                case "tmi-sent-ts":
                    twitchMessage.TmiSentTs = long.Parse(value);
                    break;
                case "turbo":
                    twitchMessage.Turbo = int.Parse(value);
                    break;

                case "user-type":
                    twitchMessage.UserType = value;
                    break;
                // Add more cases if needed...
                default: throw new Exception($"Unknown tag: {key}");
            }

            // Add more cases for other properties...
        }

        return twitchMessage;
    }
    private static TwitchMessage ParseNotification(this string[] parts)
    {
        var isTwitchNotification = !string.IsNullOrWhiteSpace(parts[1]);
        return isTwitchNotification
            ? ParseNotificationSystem(parts)
            : ParseNotificationUser(parts);
    }
    private static TwitchMessage ParseNotificationSystem(string[] parts)
    {
        var twitchMessage = new TwitchMessage
        {
            UserName = "Twitch",
            Message = parts[1].Trim(),
            MessageType = parts[1].ToMessageType()
        };
        return twitchMessage;
    }
    private static TwitchMessage ParseNotificationUser(string[] parts)
    {
        try
        {
            var messageData = parts[2].Trim().Split(' ');
            var messageType = messageData[1].ToMessageType();
            var displayName = messageData[0].Split('!')[0];
            var channelName = messageData[2].Substring(1); // Use Substring to remove '#' more efficiently

            var twitchMessage = new TwitchMessage
            {
                UserName = displayName,
                ChannelName = channelName,
                Message = parts.Length == 4 ? parts[3] : string.Join(':', parts[3..]),
                MessageType = messageType
            };
            return twitchMessage;
        }
        catch (Exception ex)
        {
            throw new Exception(string.Join(",", parts), ex);
        }

    }
    private static TwitchMessage ParseInfo(this string[] parts)
    {
        var twitchMessage = new TwitchMessage
        {
            Message = string.Join(':', parts),
            MessageType = MessageType.Info
        };
        return twitchMessage;
    }
    public static TwitchMessage ParseMessage(this string input)
    {
        //var simple = true;

        var parts = input.Split(':');
        if (parts.Length == 1) return ParseInfo(parts);
        if (parts.Length == 2) return ParseInfo(parts);
        //if (parts.Length == 3) return ParseInfo(parts);
        if (parts.Length > 3) return ParseIM(parts);
        return ParseNotification(parts);

    }

    public static CommandRequest ToRequest(this OnMessageReceivedArgs x)
    {
        var result = new CommandRequest(x.ChatMessage.Username, x.ChatMessage.Message, x.ChatMessage.RoomId.ToInt(),
            x.ChatMessage.Channel, x.ChatMessage.IsNonPleb());
        return result;
    }

    public static CommandRequest ToRequest(this OnWhisperReceivedArgs x)
    {
       var result = new CommandRequest(x.WhisperMessage.Username, x.WhisperMessage.Message, null, null, false);
       return result;
    }

}

