namespace Tennis.Models.LiveTennis
{
    public record LiveTennisPlayer
    {
        public string id { get; init; } = "";
        
        public string english_name { get; init; }

        public string simplified_name { get; init; } = "";
    }
}
