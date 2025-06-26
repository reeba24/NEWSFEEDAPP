using System.Data.SqlClient;
using System.Data;
using newsapp.Config;
using Dapper;
namespace newsapp.Data
{
    public class DataManager : IDataManager
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly ILogger<DataManager> _logger;

        public DataManager(DatabaseSettings databaseSettings, ILogger<DataManager> logger)
        {
            _databaseSettings = databaseSettings;
            _logger = logger;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                using var connection = new SqlConnection(_databaseSettings.ConnectionString);
                await connection.OpenAsync();

                return await connection.QueryAsync<T>(sql, parameters, commandType: commandType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query: {Sql}", sql);
                throw;
            }
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                using var connection = new SqlConnection(_databaseSettings.ConnectionString);
                await connection.OpenAsync();

                return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, commandType: commandType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing QueryFirstOrDefaultAsync: {Sql}", sql);
                throw;
            }
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                using var connection = new SqlConnection(_databaseSettings.ConnectionString);
                await connection.OpenAsync();

                return await connection.ExecuteAsync(sql, parameters, commandType: commandType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command: {Sql}", sql);
                throw;
            }
        }
    }

}
