namespace PolyhydraGames.Twitch.Models;

public class IgdbGame
{
    public int id { get; set; }
    public Alternative_Names[] alternative_names { get; set; }
    public int category { get; set; }
    public Collection collection { get; set; }
    public Cover cover { get; set; }
    public int created_at { get; set; }
    public int first_release_date { get; set; }
    public DateTime FirstReleaseDate => DateTimeOffset.FromUnixTimeSeconds(first_release_date).Date;
    public int[] franchises { get; set; }
    public int[] game_modes { get; set; }
    public int[] genres { get; set; }
    public Involved_Companies[] involved_companies { get; set; }
    public int[] keywords { get; set; }
    public string name { get; set; }
    public int parent_game { get; set; }
    public int[] platforms { get; set; }
    public int[] player_perspectives { get; set; }
    public int[] release_dates { get; set; }
    public Screenshot[] screenshots { get; set; }
    public int[] similar_games { get; set; }
    public string slug { get; set; }
    public string summary { get; set; }
    public int[] tags { get; set; }
    public Theme[] themes { get; set; }
    public int updated_at { get; set; }
    public string url { get; set; }
    public int[] websites { get; set; }
    public string checksum { get; set; }
}