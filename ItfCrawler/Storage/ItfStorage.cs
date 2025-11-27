using Microsoft.EntityFrameworkCore;

namespace ItfCrawler.Storage;

public class ItfStorage(ItfDbContext dbContext)
{
    public async Task SaveCountriesAsync(List<TennisCountry> countries)
    {
        var codes = countries.Select(c => c.Code);
        var existingCountries = await dbContext
            .TennisCountries
            .Where(c => codes.Contains(c.Code))
            .ToListAsync();

        foreach (var country in countries)
        {
            var existingCountry = existingCountries.FirstOrDefault(c => c.Code == country.Code);
            if (existingCountry != null)
            {
                existingCountry.Active = country.Active;
            }
            else
            {
                dbContext.TennisCountries.Add(country);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task SavePlayersAsync(List<ItfPlayer> players)
    {
        players = players.DistinctBy(p => p.Id).ToList();
        var ids = players.Select(p => p.Id);
        var existingPlayers = await dbContext
            .ItfPlayers
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        foreach (var player in players)
        {
            var existingPlayer = existingPlayers.FirstOrDefault(c => c.Id == player.Id);
            if (existingPlayer != null)
            {
                existingPlayer.Name = player.Name;
                existingPlayer.CountryCode = player.CountryCode;
                existingPlayer.OriginalUrl = player.OriginalUrl;
                existingPlayer.UpdatedDate = player.UpdatedDate;
            }
            else
            {
                dbContext.ItfPlayers.Add(player);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task SaveLeaguesAsync(List<ItfLeague> leagues)
    {
        leagues = leagues.DistinctBy(l => l.Id).ToList();
        var ids = leagues.Select(l => l.Id);
        var existingLeagues = await dbContext
            .ItfLeagues
            .Where(l => ids.Contains(l.Id))
            .ToListAsync();

        foreach (var league in leagues)
        {
            var existingLeague = existingLeagues.FirstOrDefault(l => l.Id == league.Id);
            if (existingLeague != null)
            {
                existingLeague.Name = league.Name;
                existingLeague.OriginalUrl = league.OriginalUrl;
                existingLeague.UpdatedDate = league.UpdatedDate;
            }
            else
            {
                dbContext.ItfLeagues.Add(league);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task SaveMatchesAsync(List<ItfMatch> matches)
    {
        matches = matches.DistinctBy(m => m.Id).ToList();

        var ids = matches.Select(m => m.Id);
        var existingMatches = await dbContext
            .ItfMatches
            .Where(m => ids.Contains(m.Id))
            .ToListAsync();

        foreach (var match in matches)
        {
            var existingMatch = existingMatches.FirstOrDefault(m => m.Id == match.Id);
            if (existingMatch != null)
            {
                existingMatch.HomeId1 = match.HomeId1;
                existingMatch.HomeId2 = match.HomeId2;
                existingMatch.AwayId1 = match.AwayId1;
                existingMatch.AwayId2 = match.AwayId2;
                existingMatch.LeagueId = match.LeagueId;
                existingMatch.KickOffTime = match.KickOffTime;
                existingMatch.UpdatedDate = match.UpdatedDate;
            }
            else
            {
                dbContext.ItfMatches.Add(match);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task CreateOrUpdateLeague(ItfLeague league)
    {
        var existingLeague = await dbContext
            .ItfLeagues
            .FirstOrDefaultAsync(l => l.Id == league.Id);

        if (existingLeague != null)
        {
            existingLeague.Name = league.Name;
            existingLeague.OriginalUrl = league.OriginalUrl;
            existingLeague.UpdatedDate = league.UpdatedDate;
        }
        else
        {
            dbContext.ItfLeagues.Add(league);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task CreateOrUpdateMatch(ItfMatch match)
    {
        var existingMatch = await dbContext
            .ItfMatches
            .FirstOrDefaultAsync(m => m.Id == match.Id);

        if (existingMatch != null)
        {
            existingMatch.HomeId1 = match.HomeId1;
            existingMatch.HomeId2 = match.HomeId2;
            existingMatch.AwayId1 = match.AwayId1;
            existingMatch.AwayId2 = match.AwayId2;
            existingMatch.LeagueId = match.LeagueId;
            existingMatch.KickOffTime = match.KickOffTime;
            existingMatch.UpdatedDate = match.UpdatedDate;
        }
        else
        {
            dbContext.ItfMatches.Add(match);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task CreateOrUpdatePlayer(ItfPlayer player)
    {
        var existingPlayer = await dbContext
            .ItfPlayers
            .FirstOrDefaultAsync(p => p.Id == player.Id);

        if (existingPlayer != null)
        {
            existingPlayer.Name = player.Name;
            existingPlayer.CountryCode = player.CountryCode;
            existingPlayer.OriginalUrl = player.OriginalUrl;
            existingPlayer.UpdatedDate = player.UpdatedDate;
        }
        else
        {
            dbContext.ItfPlayers.Add(player);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<ItfPlayer> GetPlayer(string name, string countryCode)
    {
        return await dbContext.ItfPlayers
            .FirstOrDefaultAsync(p => p.Name == name && p.CountryCode == countryCode);
    }
}