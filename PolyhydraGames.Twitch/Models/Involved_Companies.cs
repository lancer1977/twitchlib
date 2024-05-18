namespace PolyhydraGames.Twitch.Models;

public class Involved_Companies
{
    public int id { get; set; }
    public int company { get; set; }
    public string Name { get; set; }
    public int created_at { get; set; }
    public bool developer { get; set; }
    public int game { get; set; }
    public bool porting { get; set; }
    public bool publisher { get; set; }
    public bool supporting { get; set; }
    public int updated_at { get; set; }
    public string checksum { get; set; }
}

