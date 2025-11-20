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

    [Column("updated_date")]
    public DateTime UpdatedDate { get; set; }
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

    [Column("updated_date")] 
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}