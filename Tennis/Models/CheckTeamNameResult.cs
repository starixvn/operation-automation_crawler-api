namespace Tennis.Models;

public class CheckTeamNameResult
{
    public bool IsExist { get; set; }
    public string Message { get; set; }
    public TeamModel Team { get; set; }
}

public class TeamModel
{
    public int SportType { get; set; }
    public string TeamName { get; set; }
    public string? TeamNameCn { get; set; }
    public string TeamNameSc2 { get; set; }
    public string TeamNameUni { get; set; }
}