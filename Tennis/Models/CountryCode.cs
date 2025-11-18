namespace Tennis.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("country_code")]
public class CountryCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int Id { get; set; }

    [Column("english_name")]
    [StringLength(100)] 
    public string EnglishName { get; set; }

    [Column("country_name")]
    [StringLength(100)]
    public string CountryName { get; set; }

    [Column("traditional_name")]
    [StringLength(100)]
    public string TraditionalName { get; set; }

    [Column("simplified_name")]
    [StringLength(100)]
    public string SimplifiedName { get; set; }

    [Column("vietnamese")]
    [StringLength(100)]
    public string Vietnamese { get; set; }

    [Column("thai")]
    [StringLength(100)]
    public string Thai { get; set; }
}