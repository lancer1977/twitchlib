using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PolyhydraGames.Api.SystemDefinitions;
using PolyhydraGames.Core.Interfaces.Gaming;
using PolyhydraGames.Core.Models;

namespace PolyhydraGames.Twitch.Services;

public class IgdbClient : IGameUpdater
{
    private readonly ILogger<IgdbClient> _log;
    private readonly ICacheService _cache;
    private bool _enabled = true;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string? _token;
    private readonly HttpClient _client;

    public IgdbClient(ILogger<IgdbClient> log, IApiSettings config, ICacheService cache)
    {
        _log = log;
        _cache = cache;
        _enabled = !(string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.Secret));
        _log.LogInformation($"IGDB Client Enabled: {_enabled}");

        _token = config.AccessToken;
        _clientId = config.ClientId;
        _clientSecret = config.Secret;

        _client = new HttpClient()
        {
            BaseAddress = new Uri("https://api.igdb.com/v4/"),
            Timeout = TimeSpan.FromSeconds(5),
        };
        _client.DefaultRequestHeaders.Add("Client-ID", config.ClientId);
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public async Task SetAuthToken()
    {
        if (_enabled && !IsAuthenticated)
        {
            _log.LogInformation("Setting auth token called");
            var url = $"https://id.twitch.tv/oauth2/token?client_id={_clientId}&client_secret={_clientSecret}&grant_type=client_credentials";
            try
            {
                var result = await _client.PostAsync(url, null);
                var response = JObject.Parse(await result.Content.ReadAsStringAsync());

                _token = (response?.TryGetValue("access_token", out var token) ?? false) ? token.ToString() : null;
                _log.LogInformation(_token);

            }
            catch (Exception e)
            {
                _log.LogError(e, "Setting auth token called");

                throw;
            }


        }
    }

    private async Task<T> Post<T>(string url, string query)
    {
        if (!IsAuthenticated)
        {
            await SetAuthToken();
        }

        var content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
        _client.DefaultRequestHeaders.Add("Client-ID", _clientId);
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
        var response = await _client.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _token = string.Empty;
            _log.LogError($"Request failed: {response.ReasonPhrase}");
            return default;
        }
        else
        {
            _log.LogError($"Request failed: {response.ReasonPhrase}");
            return default;
        }
    }

    private async Task<List<IgdbGame>> GetRawGames(string name, string platform)
    {

        var url = "games";
        var companyquery = "involved_companies.*";

        var platformId = SystemHelpers.GetIgdbIdFromSlug(platform);
        if (string.IsNullOrEmpty(platformId)) throw new Exception($"Platform {platform} not found");
        var filter = $" where release_dates.platform={platformId};";
        var content = $"search \"{name}\";{filter} fields  age_ratings,aggregated_rating,aggregated_rating_count,alternative_names.*,artworks.*,bundles,category,checksum,collection.*,cover.*,created_at,dlcs,expanded_games,expansions,external_games.*,first_release_date,follows,forks,franchise,franchises,game_engines,game_localizations,game_modes,genres,hypes,{companyquery},keywords,language_supports,multiplayer_modes,name,parent_game,platforms,player_perspectives,ports,rating,rating_count,release_dates,remakes,remasters,screenshots.*,similar_games,slug,standalone_expansions,status,storyline,summary,tags,themes.*,total_rating,total_rating_count,updated_at,url,version_parent,version_title,videos,websites;limit 20;";
        _log.LogInformation($"Igdb Query: GetRawGames: {content}");
        var rawGames = await Post<List<IgdbGame>>(url, content);
        return rawGames;

    }
    public async Task<IdgbCompany> GetCompany(int id = 0)
    {
        if (!IsAuthenticated)
        {
            await SetAuthToken();
        }

        var result = await _cache.Get($"IdgbCompany{id}", async () =>
        {
            try
            {
                // where id = {id}
                var content = $"fields *;";
                if (id != 0) content += $" where id = {id};";
                var url = "companies";
                var result = await Post<JArray>(url, content);
                var first = result.First().ToString().FromJson<IdgbCompany>();

                _log.LogInformation(first.name);
                return first;

            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
                throw;
            }
             ;
        });
        return result;
    }


    public async Task<IgdbGame> GetGame(string name, string platform)
    {
        return await _cache.Get($"IgdbGame{name}{platform}", async () =>
        {
            var igdbgames = await GetRawGames(name, platform);
            var game = igdbgames.MinBy(s => s.name.LevenshteinDistance(name));

            if (game != null)
            {
                try
                {
                    var company = game.involved_companies.FirstOrDefault(x => x.developer);
                    var co = await GetCompany(company?.company ?? 0);
                    company.Name = co.name;
                    _log.LogInformation(_token);
                }
                catch (Exception e)
                {
                    _log.LogError(e, e.Message);
                }
                return game;
            }

            _log.LogInformation($"Game {name} not found on platform {platform}");
            return null;
        });

    }

    public async Task<bool> Update(IGame game)
    {
        try
        {
            var result = await GetGame(game.Title, game.Platform);
            if (result == null) return false;
            game.Description = result.summary;
            game.Year = result.FirstReleaseDate.Date.Year;
            if (string.IsNullOrEmpty(game.ImageUrl))
            {
                game.ImageUrl = result.cover?.url ?? "";
                if (game.ImageUrl.StartsWith("//"))
                    game.ImageUrl = "https:" + game.ImageUrl;
            }

            game.Developer = result.involved_companies?.FirstOrDefault(x => x.developer)?.Name;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error retrieving IGDB data: ");
            return false;
        }

        return true;
    }
    public int Priority => 1;
}

