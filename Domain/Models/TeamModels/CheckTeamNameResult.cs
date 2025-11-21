namespace Domain.Models.TeamModels;

public class CheckTeamNameResult
{
    public bool IsExist { get; set; }
    public string Message { get; set; }
    public ContentModel Team { get; set; }
}