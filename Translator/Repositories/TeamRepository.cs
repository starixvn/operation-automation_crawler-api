using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Translator.Models.TeamModels;

namespace Translator.Repositories;

public interface TeamRepository
{
    Task<ContentModel?> GetByName(string name, int sportTypeId);

    Task<ContentModel?> GetCountryCode(string name);

    Task<IEnumerable<ContentModel>> GetAllCountryCode();
}

public class TeamRepositoryImp(IConfiguration config) : TeamRepository
{
    private MySqlConnection GetConnection() => new(config.GetConnectionString("OperationCrawlerDatabase"));

    public async Task<ContentModel?> GetByName(string name, int sportTypeId)
    {
        var sql = sportTypeId switch
        {
            1 => """
                  SELECT simplified_name AS ChineseSimplified, english_name AS English, traditional_name AS ChineseTraditional
                  FROM soccer_7m
                  WHERE english_name = @name LIMIT 1
                  """,
            2 => """
                  SELECT simplified_name AS ChineseSimplified, english_name AS English, traditional_name AS ChineseTraditional
                  FROM basketball_7m
                  WHERE english_name = @name LIMIT 1
                  """,
            5 => """
                  SELECT simplified_name AS ChineseSimplified, english_name AS English
                  FROM live_tennis
                  WHERE english_name = @name LIMIT 1
                  """,
            9 => """
                  SELECT simplified_name AS ChineseSimplified, english_name AS English
                  FROM badminton
                  WHERE english_name = @name LIMIT 1
                  """,
            _ => throw new ArgumentOutOfRangeException(nameof(sportTypeId), "Unsupported sport type ID")
        };

        await using var connection = GetConnection();

        return await connection.QueryFirstOrDefaultAsync<ContentModel>(sql, new { name, sportTypeId });
    }

    public async Task<ContentModel?> GetCountryCode(string name)
    {
        const string sql = """
                           SELECT simplified_name AS ChineseSimplified, english_name AS English, traditional_name AS ChineseTraditional,
                                 vietnamese AS Vietnamese, thai AS Thai
                           FROM country_code
                           WHERE english_name = @name LIMIT 1
                           """;

        await using var connection = GetConnection();

        return await connection.QueryFirstOrDefaultAsync<ContentModel>(sql, new { name });
    }

    public async Task<IEnumerable<ContentModel>> GetAllCountryCode()
    {
        const string sql = """
                           SELECT simplified_name AS ChineseSimplified, english_name AS English, traditional_name AS ChineseTraditional,
                                 vietnamese AS Vietnamese, thai AS Thai
                           FROM country_code
                           """;

        await using var connection = GetConnection();

        return await connection.QueryAsync<ContentModel>(sql);
    }
}