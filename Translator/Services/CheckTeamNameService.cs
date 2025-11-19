using System.Text.RegularExpressions;
using Translator.Models.TeamModels;
using Translator.Repositories;

namespace Translator.Services;

public interface CheckTeamNameService
{
    Task<CheckTeamNameResult> GetTeam(string name, int sportTypeId);
}

public class CheckTeamNameServiceImp(TeamRepository teamRepository) : CheckTeamNameService
{
    public async Task<CheckTeamNameResult> GetTeam(string name, int sportTypeId)
    {
        var parts = name.Split('/').Select(p => p.Trim()).ToList();
        var translatedParts = new List<ContentModel>();
        var allCountryCode = await teamRepository.GetAllCountryCode();

        foreach (var part in parts)
        {
            var codeMatch = Regex.Match(part, @"\((?<code>[A-Z]{3})\)$");
            var isValidCountryCode = codeMatch.Success && allCountryCode.Any(x => x.English == codeMatch.Groups["code"].Value);
            if (codeMatch.Success && isValidCountryCode)
            {
                var nameOnly = part.Replace(codeMatch.Value, "").Trim();
                var countryCode = allCountryCode.FirstOrDefault(x => x.English == codeMatch.Groups["code"].Value);
                var teamName = await teamRepository.GetByName(nameOnly, sportTypeId);
                if (teamName == null)
                {
                    continue;
                }

                var finalTeamName = GetTeamNameHaveCode(teamName, countryCode);
                translatedParts.Add(finalTeamName);
            }
            else
            {
                var teamName = await teamRepository.GetByName(part, sportTypeId);
                if (teamName != null)
                {
                    translatedParts.Add(teamName);
                }
            }
        }

        if (translatedParts.Count == parts.Count && translatedParts.Count > 0)
        {
            return GetValidResult(translatedParts);
        }

        return GetInValidResult();
    }

    private static CheckTeamNameResult GetValidResult(List<ContentModel> translatedParts)
    {
        var combinedTeam = new ContentModel
        {
            English = string.Join(" / ", translatedParts.Select(p => p.English).Where(x => !string.IsNullOrWhiteSpace(x))),
            ChineseTraditional = string.Join(" / ", translatedParts.Select(p => p.ChineseTraditional).Where(x => !string.IsNullOrWhiteSpace(x))),
            ChineseSimplified = string.Join(" / ", translatedParts.Select(p => p.ChineseSimplified).Where(x => !string.IsNullOrWhiteSpace(x))),
            NewChineseSimplified = string.Join(" / ", translatedParts.Select(p => p.NewChineseSimplified).Where(x => !string.IsNullOrWhiteSpace(x))),
            Vietnamese = string.Join(" / ", translatedParts.Select(p => p.Vietnamese).Where(x => !string.IsNullOrWhiteSpace(x))),
            Thai = string.Join(" / ", translatedParts.Select(p => p.Thai).Where(x => !string.IsNullOrWhiteSpace(x))),
        };

        return new CheckTeamNameResult
        {
            IsExist = true,
            Message = "Team found",
            Team = combinedTeam,
        };
    }

    private static ContentModel GetTeamNameHaveCode(ContentModel teamName, ContentModel countryCode)
    {
        return new ContentModel
        {
            English = string.IsNullOrWhiteSpace(teamName.English)
                ? "" : teamName.English + " (" + countryCode.English + ")",
            ChineseTraditional = string.IsNullOrWhiteSpace(teamName.ChineseTraditional)
                ? "" : teamName.ChineseTraditional + " (" + countryCode.ChineseTraditional + ")",
            ChineseSimplified = string.IsNullOrWhiteSpace(teamName.ChineseSimplified)
                ? "" : teamName.ChineseSimplified + " (" + countryCode.ChineseSimplified + ")",
            NewChineseSimplified = string.IsNullOrWhiteSpace(teamName.NewChineseSimplified)
                ? "" : teamName.NewChineseSimplified + " (" + countryCode.NewChineseSimplified + ")",
            Vietnamese = string.IsNullOrWhiteSpace(teamName.Vietnamese)
                ? "" : teamName.Vietnamese + " (" + countryCode.Vietnamese + ")",
            Thai = string.IsNullOrWhiteSpace(teamName.Thai)
                ? "" : teamName.Thai + " (" + countryCode.Thai + ")",
        };
    }

    private static CheckTeamNameResult GetInValidResult()
    {
        return new CheckTeamNameResult
        {
            IsExist = false,
            Message = "Team not found!",
            Team = null,
        };
    }
}