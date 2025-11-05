namespace Tennis.Models.LiveTennis
{
    public record LiveTennisPlayer
    {
        public string id { get; init; } = "";
        
        public string url { get; init; } = "";
        
        public string name { get; init; }

        public string chinese_name { get; init; } = "";

        public string country { get; init; } = "";
    }
}
