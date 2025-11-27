using Tennis.Models;

namespace Tennis.Storage;

public class TennisDataService(TennisDbContext dbContext)
{
    public async Task SyncLeagues(List<League> leagues)
    {
        foreach (var league in leagues)
        {
            await SyncLeague(league);
            await SyncMatches(league);
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task SyncLeague(League league)
    {
        var tennisLeague = new TennisLeague
        {
            Id = $"ATP-{league.Id}",
            Name = league.Name,
            OriginalUrl = league.Source,
            UpdatedDate = DateTime.Now
        };

        var existingLeague = await dbContext.TennisLeagues.FindAsync(tennisLeague.Id);
        if (existingLeague == null)
        {
            await dbContext.TennisLeagues.AddAsync(tennisLeague);
        }
        else
        {
            existingLeague.Name = tennisLeague.Name;
            existingLeague.OriginalUrl = tennisLeague.OriginalUrl;
            existingLeague.UpdatedDate = tennisLeague.UpdatedDate;
        }
    }

    private async Task SyncMatches(League league)
    {
        var validMatches = league.Matches.Where(r => r.HomePlayers.Count > 0 || r.AwayPlayers.Count > 0);
        var matches = validMatches.Select(m => new TennisMatch
        {
            Id = $"ATP-{m.Id}",
            HomeOne = m.HomePlayers[0].Id,
            AwayOne = m.AwayPlayers[0].Id,
            HomeTwo = m.HomePlayers.Count > 1 ? m.HomePlayers[1].Id : string.Empty,
            AwayTwo = m.AwayPlayers.Count > 1 ? m.AwayPlayers[1].Id : string.Empty,
            UpdatedDate = DateTime.Now,
            KickoffTime = m.Date,
            LeagueId = $"ATP-{league.Id}"
        });

        foreach (var match in matches)
        {
            await SyncMatch(match);
        }
    }

    private async Task SyncMatch(TennisMatch match)
    {
        var existingMatch = await dbContext.TennisMatches.FindAsync(match.Id);
        if (existingMatch == null)
        {
            await dbContext.TennisMatches.AddAsync(match);
        }
        else
        {
            existingMatch.HomeOne = match.HomeOne;
            existingMatch.AwayOne = match.AwayOne;
            existingMatch.HomeTwo = match.HomeTwo;
            existingMatch.AwayTwo = match.AwayTwo;
            existingMatch.KickoffTime = match.KickoffTime;
            existingMatch.LeagueId = match.LeagueId;
            existingMatch.UpdatedDate = DateTime.Now;
        }
    }
}