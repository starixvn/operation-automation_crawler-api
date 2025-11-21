using System.Text.RegularExpressions;
using TennisWorker.Models;
using TennisWorker.Models.ATP;

namespace TennisWorker.Services;

public class ScoreConverter
{
    public static List<LiveScore> ConvertATPMatchScore(ATPMatch atpMatch)
    {
        if (atpMatch is null)
        {
            return [];
        }

        var scoresBySet = new Dictionary<int, LiveScore>();
        var matchId = CreateMatchId(atpMatch);
        AddScore(scoresBySet, atpMatch.PlayerTeam.SetScores, matchId, atpMatch.Source);
        AddScore(scoresBySet, atpMatch.OpponentTeam.SetScores, matchId, atpMatch.Source, false);

        return scoresBySet
            .OrderBy(pair => pair.Key)
            .Select(pair => pair.Value)
            .ToList();
    }

    private static string CreateMatchId(ATPMatch atpMatch)
    {
        var players = new List<string>
        {
            atpMatch.PlayerTeam.Player.PlayerId,
            atpMatch.PlayerTeam.Partner.PlayerId,
            atpMatch.OpponentTeam.Player.PlayerId,
            atpMatch.OpponentTeam.Partner.PlayerId
        };
        var playerList = string.Join("-", players.Where(r => r is not null));
        var matchId = $"{atpMatch.LeagueId}-{playerList}-{ConvertRound(atpMatch.RoundName)}";
        return matchId;
    }

    private static void AddScore(Dictionary<int, LiveScore> scoresBySet, List<ATPScore> setScores, string matchId, string source, bool isHome = true)
    {
        foreach (var set in setScores)
        {
            if (set?.SetScore is null)
            {
                continue;
            }

            var liveScore = GetOrCreate(scoresBySet, set.SetNumber, matchId, source);

            if (isHome)
            {
                liveScore.HomeValue = set.SetScore.Value.ToString();
            }
            else
            {
                liveScore.AwayValue = set.SetScore.Value.ToString();
            }
        }
    }

    private static LiveScore GetOrCreate(Dictionary<int, LiveScore> scoresBySet, int setNumber, string matchId, string source)
    {
        if (!scoresBySet.TryGetValue(setNumber, out var liveScore))
        {
            liveScore = new LiveScore { MatchId = matchId, SetNumber = setNumber, Source = source };
            scoresBySet[setNumber] = liveScore;
        }
        return liveScore;
    }

    private static string ConvertRound(string roundName)
    {
        if (string.IsNullOrWhiteSpace(roundName))
        {
            return roundName;
        }

        if (roundName.Contains("Robin"))
        {
            return "";
        }

        var normalized = roundName.Trim();
        var roundOfMatch = Regex.Match(normalized, @"^round of\s+(\d+)", RegexOptions.IgnoreCase);
        if (roundOfMatch.Success)
        {
            return $"R{roundOfMatch.Groups[1].Value}";
        }

        var normalizedLower = normalized.ToLowerInvariant();

        return normalizedLower switch
        {
            "Round Robin" => "RR",
            "Quarterfinals" => "QF",
            "Semifinals" => "SF",
            "Finals" => "F",
            _ => normalized
        };
    }
}


