
//namespace Spotabot.Test
//{
//    class FakeTwitchEventSource
//    {
//        // Define an asynchronous event
//        public event AsyncEventHandler<OnMessageReceivedArgs> MyEvent;

//        // Method to trigger the asynchronous event
//        public async Task TriggerEventAsync()
//        {
//            await Task.Delay(1000); // Simulate some asynchronous operation
//            MyEvent?.Invoke(this, new OnMessageReceivedArgs(null));
//        }
//    }
//}
//    class FakeJoinedTwitchEventSource
//    {
//        // Define an asynchronous event
//        public event AsyncEventHandler<OnJoinedChannelArgs> MyEvent;

//        // Method to trigger the asynchronous event
//        public async Task TriggerEventAsync()
//        {
//            await Task.Delay(1000); // Simulate some asynchronous operation
//            MyEvent?.Invoke(this, new OnJoinedChannelArgs(  "DreadBreadcrumb","DreadbreadBot"));
//        }
//    }
//}
//public class EventTests
//{

//    [Test]
//    public async Task TwitchMessageTestAsync()
//    {
//        var source = new FakeTwitchEventSource();

//        // Create an observable sequence from the asynchronous event handler
//        var observable = TwitchExtensions.FromEventPattern<OnMessageReceivedArgs>(
//            handler => source.MyEvent += handler,
//            handler => source.MyEvent -= handler
//        );

//        // Subscribe to the observable sequence
//        observable.Subscribe(ep =>
//        {
//            Console.WriteLine("Event occurred");
//            Assert.Pass();
//        });

//        // Trigger the asynchronous event
//        await source.TriggerEventAsync();

//    }

//    [Test]
//    public async Task TwitchJoinedMessageTestAsync()
//    {
//        var source = new FakeJoinedTwitchEventSource();

//        // Create an observable sequence from the asynchronous event handler
//        var observable = TwitchExtensions.FromEventPattern<OnJoinedChannelArgs>(
//            handler => source.MyEvent += handler,
//            handler => source.MyEvent -= handler
//        );

//        // Subscribe to the observable sequence
//        observable.Subscribe(ep =>
//        {
//            Console.WriteLine("Event occurred");
//            Assert.That(ep.Channel == "Dreadbread");
//        });

//        // Trigger the asynchronous event
//        await source.TriggerEventAsync();

//    }
//}