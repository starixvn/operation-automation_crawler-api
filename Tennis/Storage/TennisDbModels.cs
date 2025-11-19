using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Tennis.Storage;

public class TennisLeague
{
    [Key]
    [Column("id")]
    [StringLength(100)]
    public string Id { get; set; } = string.Empty;

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("original_url")]
    [StringLength(4000)]
    public string OriginalUrl { get; set; }

    [Column("created_date")]
    public DateTime? CreatedDate { get; set; }

    public ICollection<TennisMatch> Matches { get; set; } = new List<TennisMatch>();
}

public class TennisMatch
{
    [Key]
    [Column("id")]
    [StringLength(100)]
    public string Id { get; set; } = string.Empty;

    [Column("home")]
    [StringLength(255)]
    public string Home { get; set; } = string.Empty;

    [Column("away")]
    [StringLength(255)]
    public string Away { get; set; } = string.Empty;

    [Column("league_id")]
    [StringLength(100)]
    public string LeagueId { get; set; } = string.Empty;

    [Column("kickoff_time")] 
    public DateTime KickoffTime { get; set; } = DateTime.MaxValue;

    [Column("created_date")] 
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public TennisLeague League { get; set; } = null!;
}