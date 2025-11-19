using Microsoft.EntityFrameworkCore;
using Translator.Models;
using Translator.Models.LiveTennis;

namespace Translator.Storage;

public class DataService(OperationCrawlerDbContext dbContext)
{
    public async Task<List<LiveTennisPlayer>> UpdateLiveTennis(List<LiveTennisPlayer> players)
    {
        await RemoveCountryCodeFromName(players);

        var idsFromDb = await dbContext.LiveTennisPlayers.Select(m => m.id).ToListAsync();
        var playerIds = players.Select(p => p.id).ToList();
        var existingIds = idsFromDb.Intersect(playerIds);

        var playersToAdd = players
            .Where(p => !existingIds.Contains(p.id))
            .ToList();

        var playersToUpdate = players
            .Where(p => existingIds.Contains(p.id))
            .ToList();

        if (playersToAdd.Any())
        {
            await dbContext.LiveTennisPlayers.AddRangeAsync(playersToAdd);
            await dbContext.SaveChangesAsync();
        }

        if (playersToUpdate.Any())
        {
            dbContext.LiveTennisPlayers.UpdateRange(playersToUpdate);
            await dbContext.SaveChangesAsync();
        }

        return playersToAdd;
    }

    public async Task<List<Soccer7m>> UpdateSoccer7m(List<Soccer7m> players)
    {
        await RemoveCountryCodeFromName(players);

        var idsFromDb = await dbContext.Soccer7m.Select(m => m.id).ToListAsync();
        var playerIds = players.Select(p => p.id).ToList();
        var existingIds = idsFromDb.Intersect(playerIds);

        var playersToAdd = players
            .Where(p => !existingIds.Contains(p.id))
            .ToList();

        var playersToUpdate = players
            .Where(p => existingIds.Contains(p.id))
            .ToList();

        if (playersToAdd.Any())
        {
            await dbContext.Soccer7m.AddRangeAsync(playersToAdd);
            await dbContext.SaveChangesAsync();
        }

        if (playersToUpdate.Any())
        {
            dbContext.Soccer7m.UpdateRange(playersToUpdate);
            await dbContext.SaveChangesAsync();
        }

        return playersToAdd;
    }

    public async Task<List<Basketball7m>> UpdateBasketball7m(List<Basketball7m> players)
    {
        await RemoveCountryCodeFromName(players);

        var idsFromDb = await dbContext.Basketball7m.Select(m => m.id).ToListAsync();
        var playerIds = players.Select(p => p.id).ToList();
        var existingIds = idsFromDb.Intersect(playerIds);

        var playersToAdd = players
            .Where(p => !existingIds.Contains(p.id))
            .ToList();

        var playersToUpdate = players
            .Where(p => existingIds.Contains(p.id))
            .ToList();

        if (playersToAdd.Any())
        {
            await dbContext.Basketball7m.AddRangeAsync(playersToAdd);
            await dbContext.SaveChangesAsync();
        }

        if (playersToUpdate.Any())
        {
            dbContext.Basketball7m.UpdateRange(playersToUpdate);
            await dbContext.SaveChangesAsync();
        }

        return playersToAdd;
    }

    private async Task RemoveCountryCodeFromName<T>(List<T> players) where T:class, IHasEnglishName
    {
        var countries = await dbContext.CountryCodes.Select(c => $"({c.EnglishName})").ToListAsync();

        players.ForEach(p =>
        {
            var name = p.english_name.Trim();
            p.english_name = RemoveCountryCodeFromName(countries, name);
        });
    }

    public async Task<List<Badminton>> UpdateBadminton(List<Badminton> players)
    {
        await RemoveCountryCodeFromName(players);

        var idsFromDb = await dbContext.Badminton.Select(m => m.id).ToListAsync();
        var playerIds = players.Select(p => p.id).ToList();
        var existingIds = idsFromDb.Intersect(playerIds);

        var playersToAdd = players
            .Where(p => !existingIds.Contains(p.id))
            .ToList();

        var playersToUpdate = players
            .Where(p => existingIds.Contains(p.id))
            .ToList();

        if (playersToAdd.Any())
        {
            await dbContext.Badminton.AddRangeAsync(playersToAdd);
            await dbContext.SaveChangesAsync();
        }

        if (playersToUpdate.Any())
        {
            dbContext.Badminton.UpdateRange(playersToUpdate);
            await dbContext.SaveChangesAsync();
        }

        return playersToAdd;
    }

    private static string RemoveCountryCodeFromName(List<string> countries, string name)
    {
        foreach (var code in countries)
        {
            if (name.EndsWith(code))
            {
                name = name.Replace(code, "").Trim();
                break;
            }
        }

        return name;
    }
}