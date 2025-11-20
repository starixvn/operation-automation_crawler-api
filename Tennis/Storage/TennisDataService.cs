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
        var matches = league.Matches.Select(m => new TennisMatch
        {
            Id = $"ATP-{m.Id}",
            Home = m.Home,
            Away = m.Away,
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
            existingMatch.Home = match.Home;
            existingMatch.Away = match.Away;
            existingMatch.KickoffTime = match.KickoffTime;
            existingMatch.LeagueId = match.LeagueId;
            existingMatch.UpdatedDate = DateTime.Now;
        }
    }
}