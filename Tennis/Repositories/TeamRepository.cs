#nullable enable
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Tennis.Models;

namespace Tennis.Repositories;

public interface TeamRepository
{
    Task<TeamModel?> GetByName(string name, int sportTypeId);
}

public class TeamRepositoryImp(IConfiguration config) : TeamRepository
{
    private MySqlConnection GetConnection() => new(config.GetConnectionString("OperationCrawlerDatabase"));

    public async Task<TeamModel?> GetByName(string name, int sportTypeId)
    {
        var sql = sportTypeId switch
        {
            1 => """
                  SELECT simplified_name AS TeamNameSc2, english_name AS TeamName, traditional_name AS TeamNameCn
                  FROM soccer_7m
                  WHERE english_name = @name LIMIT 1
                  """,
            2 => """
                  SELECT simplified_name AS TeamNameSc2, english_name AS TeamName, traditional_name AS TeamNameCn
                  FROM basketball_7m
                  WHERE english_name = @name LIMIT 1
                  """,
            5 => """
                  SELECT simplified_name AS TeamNameSc2, english_name AS TeamName
                  FROM live_tennis
                  WHERE english_name = @name LIMIT 1
                  """,
            9 => """
                  SELECT simplified_name AS TeamNameSc2, english_name AS TeamName
                  FROM badminton
                  WHERE english_name = @name LIMIT 1
                  """,
            _ => throw new ArgumentOutOfRangeException(nameof(sportTypeId), "Unsupported sport type ID")
        };

        await using var connection = GetConnection();

        return await connection.QueryFirstOrDefaultAsync<TeamModel>(sql, new { name });
    }
}