namespace Tennis.Models.LiveTennis
{
    public class LiveTennisPlayer: IHasEnglishName
    {
        public string id { get; set; } = "";
        
        public string english_name { get; set; }

        public string simplified_name { get; set; } = "";
    }
}
