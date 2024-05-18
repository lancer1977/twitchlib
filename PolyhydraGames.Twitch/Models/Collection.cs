namespace PolyhydraGames.Twitch.Models;

public class Collection
{
    public int id { get; set; }
    public int created_at { get; set; }
    public int[] games { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public int updated_at { get; set; }
    public string url { get; set; }
    public string checksum { get; set; }
}