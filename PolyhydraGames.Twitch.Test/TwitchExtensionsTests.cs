using PolyhydraGames.Streaming.Interfaces.Enums;

namespace PolyhydraGames.Twitch.Test;

[TestFixture]
public class TwitchExtensionsTests
{
    private int performancecount = 100000;
    //[TestCase(@"Received: :streamelements!streamelements@streamelements.tmi.twitch.tv JOIN #dreadbreadbot", ExpectedResult = "dreadbreadbot"),
    //    TestCase(@"Received: @badge-info=;badges=moderator/1;client-nonce=1fa7ffc7a25fda8f64ce57b57910cb95;color=;display-name=DreadBreadboT;emotes=;first-msg=0;flags=;id=784a007a-f811-4497-abd1-82fc26fb03fc;mod=1;returning-chatter=0;room-id=141879576;subscriber=0;tmi-sent-ts=1705039472792;turbo=0;user-id=115534815;user-type=mod :dreadbreadbot!dreadbreadbot@dreadbreadbot.tmi.twitch.tv PRIVMSG #dreadbreadcrumb :hi there", ExpectedResult = "dreadbreadcrumb"),
    //TestCase(@"Received: @badge-info=;badges=broadcaster/1;client-nonce=5b025b6301754af2b5e331c72217956c;color=#D2691E;display-name=DreadBreadCrumb;emotes=;first-msg=0;flags=;id=862b010a-c1ab-439b-bb22-1d661b487917;mod=0;returning-chatter=0;room-id=141879576;subscriber=0;tmi-sent-ts=1705039821378;turbo=0;user-id=141879576;user-type= :dreadbreadcrumb!dreadbreadcrumb@dreadbreadcrumb.tmi.twitch.tv PRIVMSG #dreadbreadcrumb :#you stink", ExpectedResult = "dreadbreadcrumb"),
    //TestCase(@"Received: @badge-info=;badges=broadcaster/1;client-nonce=32c0892f9b677c426ff0c9bc4594d25e;color=#D2691E;display-name=DreadBreadCrumb;emotes=;first-msg=0;flags=;id=83376fd0-0950-41f1-b245-525f1d2b2d30;mod=0;returning-chatter=0;room-id=141879576;subscriber=0;tmi-sent-ts=1705039925598;turbo=0;user-id=141879576;user-type= :dreadbreadcrumb!dreadbreadcrumb@dreadbreadcrumb.tmi.twitch.tv PRIVMSG #dreadbreadcrumb :############### Youu ### STINK!!!!! ::::", ExpectedResult = "dreadbreadcrumb"),

    //]
    //public string PositiveChannelHits(string message)
    //{
    //    return message.GetChannelName();
    //}

    //[TestCase(@"Received: @badge-info=;badges=moderator/1;client-nonce=1fa7ffc7a25fda8f64ce57b57910cb95;color=;display-name=DreadBreadboT;emotes=;first-msg=0;flags=;id=784a007a-f811-4497-abd1-82fc26fb03fc;mod=1;returning-chatter=0;room-id=141879576;subscriber=0;tmi-sent-ts=1705039472792;turbo=0;user-id=115534815;user-type=mod :dreadbreadbot!dreadbreadbot@dreadbreadbot.tmi.twitch.tv PRIVMSG #dreadbreadcrumb :hi there")]
    //public void TestRegex(string value)
    //{

    //    SpeedTest(() =>
    //    {
    //        var result = value.GetChannelName();
    //        Assert.That(result, Is.EqualTo("dreadbreadcrumb"));
    //    });

    //}

    [TestCase(@"Received: @badge-info=;badges=moderator/1;client-nonce=1fa7ffc7a25fda8f64ce57b57910cb95;color=;display-name=DreadBreadboT;emotes=;first-msg=0;flags=;id=784a007a-f811-4497-abd1-82fc26fb03fc;mod=1;returning-chatter=0;room-id=141879576;subscriber=0;tmi-sent-ts=1705039472792;turbo=0;user-id=115534815;user-type=mod :dreadbreadbot!dreadbreadbot@dreadbreadbot.tmi.twitch.tv PRIVMSG #dreadbreadcrumb :hi there")]
    public void TestMessage(string value)
    {

        SpeedTest(() =>
        {
            var result = value.ParseMessage();
            //Assert.That(result.DisplayName, Is.EqualTo("dreadbreadcrumb"));
            Assert.That(!string.IsNullOrEmpty(result.UserName));
        });
    }

    private void SpeedTest(Action act)
    {
        var starttime = DateTime.Now;
        for (var i = 0; i < performancecount; i++)
        {
            act.Invoke();
        }
        var endtime = DateTime.Now;
        var elapsed = endtime - starttime;
        Console.WriteLine($"{performancecount}: Duration: {elapsed.ToString()}");
    }

    [

        TestCase("Received: :streamelements!streamelements@streamelements.tmi.twitch.tv JOIN #dreadbreadbot", ExpectedResult = "dreadbreadbot"),
        TestCase("Received: :streamelements!streamelements@streamelements.tmi.twitch.tv JOIN #dreadbreadbot", ExpectedResult = "dreadbreadbot"),
        TestCase(@"Received: :dreadbreadcrumb!dreadbreadcrumb@dreadbreadcrumb.tmi.twitch.tv JOIN #dreadbreadcrumb", ExpectedResult = "dreadbreadcrumb"),
        TestCase(@"Received: :dreadbreadbot!dreadbreadbot@dreadbreadcrumb.tmi.twitch.tv JOIN #dreadbreadcrumb", ExpectedResult = "dreadbreadcrumb"),
    //TestCase(@"", ExpectedResult = ""),
    //TestCase(@"", ExpectedResult = "")
    ]
    public string JoinCalls(string message)
    {
        var msg = message.ParseMessage();
        Assert.That(msg.MessageType, Is.EqualTo(MessageType.Join));
        return msg.ChannelName;
    }

    [
        TestCase(@"Received: :tmi.twitch.tv 001 dreadbreadbot :Welcome, GLHF!", ExpectedResult = "dreadbreadbot"),
        TestCase(@"Received: :tmi.twitch.tv 002 dreadbreadbot :Your host is tmi.twitch.tv", ExpectedResult = "dreadbreadbot"),
        TestCase(@"Received: :tmi.twitch.tv 003 dreadbreadbot :This server is rather new", ExpectedResult = "dreadbreadbot")
    ]
    public string NamedInfoCalls(string message)
    {
        var msg = message.ParseMessage();
        Assert.That(msg.MessageType, Is.EqualTo(MessageType.Info));
        return msg.ChannelName;
    }

    [
    TestCase("Connecting to: wss://irc-ws.chat.twitch.tv:443"),
    TestCase(@"TwitchLib-TwitchClient initialized, assembly version: 3.3.1.0")]
    public void InfoCalls(string message)
    {
        var msg = message.ParseMessage();
        Assert.That(msg.MessageType, Is.EqualTo(MessageType.Info));
    }

    [TestCase(@"Received: PONG :tmi.twitch.tv")]
    public void PongCalls(string message)
    {
        var msg = message.ParseMessage();
        Assert.That(msg.MessageType, Is.EqualTo(MessageType.Pong));
    }
    [TestCase(@"Received: PING :tmi.twitch.tv")]
    public void PingCalls(string message)
    {
        var msg = message.ParseMessage();
        Assert.That(msg.MessageType, Is.EqualTo(MessageType.Ping));
    }
}