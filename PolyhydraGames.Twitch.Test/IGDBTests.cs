using TwitchLib.Api.Core.Interfaces;

namespace PolyhydraGames.Twitch.Test;

[TestFixture]
public class IGDBTests : TestBase
{ 

    [SetUp]
    public async Task SetupAsync()
    {
        await SystemsDatabase.Instance.Initialize();
    }

    public IgdbClient IgdbService { get; set; }

    public IGDBTests()
    {
        //var logClientConfig = new Moq.Mock<ILogger<ITwitchClientConfig>>().Object;
        //var logClient = new Moq.Mock<ILogger<IgdbClient>>().Object; 
        var host = TestFixtures.GetHost((ctx, services) =>
        {
            services.AddSingleton(new HttpClient()); 
            services.AddSingleton< IApiSettings, TwitchApiConfig >();
            //services.AddSingleton(logClient);
            //services.AddSingleton(logClientConfig); 
            services.AddSingleton<ICacheService, FakeCacheService>(); 
            services.AddSingleton<IgdbClient>();
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis.polyhydragames.com"));
        });
        IgdbService = host.Services.GetService<IgdbClient>();
    }

    [TestCase(395, ExpectedResult = "Sonic Team"),
     TestCase(34064, ExpectedResult = "Tiger Electronics"),
     TestCase(38808, ExpectedResult = "puyofan99"),]
    public async Task<string> GetDeveloper(int id)
    {
        var games = await IgdbService.GetCompany(id);

        return games.name;

    }

    [TestCase("Sonic the Hedgehog", "genesis", ExpectedResult = 1991),
     TestCase("Rocket Knight Adventures", "genesis", ExpectedResult = 1993)]
    public async Task<int> GetYear(string name, string platform)
    {
        var games = await IgdbService.GetGame(name, platform);
        Console.WriteLine(games.ToString());
        Console.WriteLine(games.FirstReleaseDate.Year);
        Assert.That(!string.IsNullOrEmpty(games.summary));
        Console.WriteLine(games.involved_companies.First(x => x.developer).ToJson());
        return games.FirstReleaseDate.Year;

    }

    [TestCase("Sonic the Hedgehog", "genesis", ExpectedResult = 1991),
     TestCase("Rocket Knight Adventures", "genesis", ExpectedResult = 1993)]
    public async Task<int> OnGameChange(string name, string platform)
    {
    
        var games = await IgdbService.GetGame(name, platform);
  
        Console.WriteLine(games.ToString());
        Console.WriteLine(games.FirstReleaseDate.Year);
        Assert.That(!string.IsNullOrEmpty(games.summary));
        Console.WriteLine(games.involved_companies.First());
        return games.FirstReleaseDate.Year;
    }
 
}