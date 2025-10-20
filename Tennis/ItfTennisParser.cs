using Tennis.Extensions;
using Tennis.Models;
using Tennis.Models.ItfTennisModels;

namespace Tennis;

public class ItfTennisParser(TimeIsCrawler timeIsCrawler)
{
    public async Task<List<League>> Parse(List<ItfTennisLeague> itfLeagues, List<string> activeFlags)
    {
        itfLeagues = itfLeagues
            .Where(i => i.tourStatusDesc != "CANCELLED").ToList();
        var leagues = new List<League>();

        foreach (var tournament in itfLeagues)
        {
            var (dateFrom, dateTo) = DateTimeExtensions.ParseItfDateRange(tournament.dates);
            var league = new League
            {
                Name = tournament.tournamentName,
                Id = tournament.tournamentKey,
                Source = $"https://www.itftennis.com{tournament.tournamentLink}",
                Date = tournament.dates,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Location = tournament.location,
                Country = tournament.hostNation,
                Group = tournament.tennisCategoryCode,
                Matches = await ParseMatches(activeFlags, tournament),
                Players = ParsePlayers(activeFlags, tournament),
                MatchTypes = tournament.matches.Select(m => m.eventDesc).Distinct().ToList()
            };

            leagues.Add(league);
        }

        return leagues;
    }

    private async Task<List<Match>> ParseMatches(List<string> activeFlags, ItfTennisLeague tournament)
    {
        var itfMatchesForLeague = tournament.matches ?? [];
        var matches = new List<Match>();
        foreach (var m in itfMatchesForLeague)
        {
            var match = await CreateMatch(m, tournament, activeFlags);
            matches.Add(match);
        }

        return matches;
    }

    private List<string> ParsePlayers(List<string> activeFlags, ItfTennisLeague tournament)
    {
        var itfPlayersForLeague = tournament.players ?? [];
        var players =  MapPlayersFromTeam(itfPlayersForLeague);

        var playerNames = players.Select(p =>
        {
            var flag = "flag-" + p.Nationality.ToLower();
            return activeFlags.Contains(flag) ? $"{p.Name} ({p.Nationality})" : p.Name;
        }).ToList();

        return playerNames;
    }

    private async Task<Match> CreateMatch(ItfTennisMatch match, ItfTennisLeague league, List<string> activeFlags)
    {
        var leagueUrl = $"https://www.itftennis.com{league.tournamentLink}";
        var matchTime = ParseMatchTime(match.schedule, match.playDate);

        var homePlayers = MapPlayersFromTeam(match.homePlayers);
        var awayPlayers = MapPlayersFromTeam(match.awayPlayers);

        var result = new Match(match.matchId.ToString())
        {
            OriginalDate = matchTime,
            Date = matchTime,
            Location = "NA",
            Home = FormatPlayerNames(homePlayers, activeFlags),
            Away = FormatPlayerNames(awayPlayers, activeFlags),
            Source = leagueUrl,
            MatchType = match.eventDesc
        };

        var timeIsModel = await timeIsCrawler.GetTimeIsModel(league.venue);
        result.Country = league.hostNation;
        result.City = league.venue;

        if (timeIsModel != null)
        {
            result.UtcOffset = timeIsModel.UtcOffset;
            result.Date = matchTime.AddHours(-result.UtcOffset + 8);
            result.UtcCity = timeIsModel.Location;
            result.Group = league.tennisCategoryCode;
        }

        return result;
    }

    private DateTime ParseMatchTime(string schedule, DateTime date)
    {
        var matchTime = date;
        if (schedule.StartsWith("Starting at "))
        {
            var timeStr = schedule.Replace("Starting at ", "");
            if (TimeSpan.TryParse(timeStr, out var time))
            {
                matchTime = date.Add(time);
            }
        }
        else if (schedule.StartsWith("Not Before "))
        {
            var timeStr = schedule.Replace("Not Before ", "");
            if (TimeSpan.TryParse(timeStr, out var time))
            {
                matchTime = date.Add(time);
            }
        }
        return matchTime;
    }

    private List<Player> MapPlayersFromTeam(List<ItfTennisPlayer> teamPlayers)
    {
        return teamPlayers
            .Where(p => p != null)
            .Select(p => new Player(p!.playerId.ToString())
            {
                Name = $"{p.givenName} {p.familyName}".Trim(),
                Nationality = p.nationality,
                Source = p.profileLink
            })
            .ToList();
    }

    private string FormatPlayerNames(List<Player> players, List<string> activeFlags)
    {
        return string.Join("/", players.Select(p =>
        {
            var flag = "flag-" + p.Nationality.ToLower();

            return activeFlags.Contains(flag) ? $"{p.Name} ({p.Nationality})" : p.Name;
        }));
    }
}