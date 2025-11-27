using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItfCrawler.Storage;

// --- ITF League Entity Model ---
[Table("itf_leagues")]
public class ItfLeague
{
    // Primary Key: 'id' (varchar(45))
    [Key]
    [Column("id", TypeName = "varchar(45)")]
    public string Id { get; set; } = "0"; // Matches NOT NULL DEFAULT '0'

    // League name (varchar(400))
    [Column("name", TypeName = "varchar(400)")]
    public string Name { get; set; } = ""; // Matches NOT NULL DEFAULT ''

    // Original URL (varchar(400))
    [Column("original_url", TypeName = "varchar(400)")]
    public string OriginalUrl { get; set; } = ""; // Matches NOT NULL DEFAULT ''

    // Last update timestamp (datetime). DEFAULT CURRENT_TIMESTAMP is handled by the database.
    [Column("updated_date", TypeName = "datetime")]
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}

// --- ITF Match Entity Model ---
[Table("itf_matches")]
public class ItfMatch
{
    // Primary Key: 'id' (int(11))
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] // Assuming ID is not auto-incremented
    public int Id { get; set; }

    // Home player ID 1 (int(11))
    [Column("home_id1")]
    public int HomeId1 { get; set; } = 0; // Matches NOT NULL DEFAULT '0'

    // Home player ID 2 (int(11))
    [Column("home_id2")]
    public int HomeId2 { get; set; } = 0; // Matches NOT NULL DEFAULT '0'

    // Away player ID 1 (int(11))
    [Column("away_id1")]
    public int AwayId1 { get; set; } = 0; // Matches NOT NULL DEFAULT '0'

    // Away player ID 2 (int(11))
    [Column("away_id2")]
    public int AwayId2 { get; set; } = 0; // Matches NOT NULL DEFAULT '0'

    // League ID (varchar(45)) - Treated as a regular property (no foreign key relationship)
    [Column("league_id", TypeName = "varchar(45)")]
    public string LeagueId { get; set; } = ""; // Matches NOT NULL DEFAULT ''

    // Kick off time (datetime). Default value '2100-01-01 00:00:00' is handled by the database.
    [Column("kick_off_time", TypeName = "datetime")]
    public DateTime KickOffTime { get; set; } = new DateTime(2100, 1, 1, 0, 0, 0);

    // Last update timestamp (datetime). DEFAULT CURRENT_TIMESTAMP is handled by the database.
    [Column("updated_date", TypeName = "datetime")]
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}

// --- ITF Player Entity Model ---
[Table("itf_players")]
public class ItfPlayer
{
    // Primary Key: 'id' (int(11))
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] // Assuming ID is not auto-incremented
    public long Id { get; set; }

    // Player name (varchar(100))
    [Column("name", TypeName = "varchar(100)")]
    public string Name { get; set; } = ""; // Matches NOT NULL DEFAULT ''

    // Country code (varchar(10)) - Treated as a regular property (no foreign key relationship)
    [Column("country_code", TypeName = "varchar(10)")]
    public string CountryCode { get; set; } = ""; // Matches NOT NULL DEFAULT ''

    // Original URL (varchar(400))
    [Column("original_url", TypeName = "varchar(400)")]
    public string OriginalUrl { get; set; } = ""; // Matches NOT NULL DEFAULT ''

    // Last update timestamp (datetime). DEFAULT CURRENT_TIMESTAMP is handled by the database.
    [Column("updated_date", TypeName = "datetime")]
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}

// --- Tennis Country Entity Model ---
[Table("tennis_countries")]
public class TennisCountry
{
    // Primary Key: 'code' (varchar(10))
    [Key]
    [Column("code", TypeName = "varchar(10)")]
    public string Code { get; set; } = string.Empty;

    // Active status (tinyint(1)). Mapped to bool for convenience in C#.
    [Column("active", TypeName = "tinyint(1)")]
    public bool Active { get; set; } = false; // Matches NOT NULL DEFAULT '0'
}
