using Microsoft.Extensions.DependencyInjection;

namespace PolyhydraGames.Twitch.Setup;
public static class Bootstrapper
{

    public static IServiceCollection AddConfig(this IServiceCollection services)
    {

        services.AddSingleton<IConfiguration>(GetConfiguration());
        return services;
    }

    public static IConfigurationRoot GetConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets("65a2f916-1765-44e8-8d59-2d2ddcd7cc9b") // Use the UserSecretsId generated earlier
                                                                    //		<UserSecretsId>65a2f916-1765-44e8-8d59-2d2ddcd7cc9b</UserSecretsId>
            .Build();
        return config;
    }
}