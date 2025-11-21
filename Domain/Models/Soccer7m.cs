namespace Domain.Models;
public interface IHasEnglishName
{
    string english_name { get; set; }
}

public class Soccer7m : IHasEnglishName
{
    public int id { get; set; }

    public string english_name { get; set; } = "";

    public string traditional_name { get; set; } = "";

    public string simplified_name { get; set; } = "";
}


public class Basketball7m : IHasEnglishName
{
    public int id { get; set; }

    public string english_name { get; set; } = "";

    public string traditional_name { get; set; } = "";

    public string simplified_name { get; set; } = "";
}

