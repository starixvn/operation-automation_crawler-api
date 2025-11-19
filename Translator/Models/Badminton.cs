namespace Translator.Models;

public class Badminton: IHasEnglishName
{
    public int id { get; set; } = 0;

    public string simplified_name { get; set; } = "";

    public string english_name { get; set; } = "";
}