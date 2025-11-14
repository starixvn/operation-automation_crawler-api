using Tennis.Models;
using Tennis.Repositories;

namespace Tennis.Services;

public interface CheckTeamNameService
{
    Task<CheckTeamNameResult> GetTeam(string name, int sportTypeId);
}

public class CheckTeamNameServiceImp(TeamRepository teamRepository) : CheckTeamNameService
{
    public async Task<CheckTeamNameResult> GetTeam(string name, int sportTypeId)
    {
        var result = await teamRepository.GetByName(name, sportTypeId);

        var response = new CheckTeamNameResult
        {
            IsExist = result != null,
            Message = result != null ? "Team found" : "Team not found",
            Team = result
        };

        return response;
    }
}