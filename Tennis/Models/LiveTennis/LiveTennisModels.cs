namespace Tennis.Models.LiveTennis
{
    public record LiveTennisPlayer
    {
        public string id { get; init; } = "";
        public string ioc { get; init; } = "";
        public string name { get; init; }
        
        public string en_nation { get; init; } = "";
        public string zh_nation { get; init; } = "";
        public string ja_nation { get; init; } = "";
    
        public string en_full_name { get; init; } = "";
        public string zh_full_name { get; init; } = "";
        public string ja_full_name { get; init; } = "";
        
        public string en_name { get; set; }
        public string zh_name { get; init; } = "";
        public string ja_name { get; init; } = "";
    }
}
