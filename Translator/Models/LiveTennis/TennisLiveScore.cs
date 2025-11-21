namespace Tennis.Models.LiveTennis;


public class TennisLiveScore
{
    public int Id { get; set; }
    public string match_id { get; set; }
    public string home_value { get; set; }
    public string away_value { get; set; }
    public int set_number { get; set; }

    public string source { get; set; }

    public DateTime created_date { get; set; }
}