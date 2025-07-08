using System.Data;
using System.Data.SqlClient;
using Dapper;
using newsapp.Config;

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

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_databaseSettings.ConnectionString);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                using var connection = (SqlConnection)CreateConnection();
                await connection.OpenAsync();
                return await connection.QueryAsync<T>(sql, parameters, commandType: commandType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing QueryAsync: {Sql}", sql);
                throw;
            }
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                using var connection = (SqlConnection)CreateConnection();
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
                using var connection = (SqlConnection)CreateConnection();
                await connection.OpenAsync();
                return await connection.ExecuteAsync(sql, parameters, commandType: commandType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing ExecuteAsync: {Sql}", sql);
                throw;
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                using var connection = (SqlConnection)CreateConnection();
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<T>(sql, parameters, commandType: commandType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing ExecuteScalarAsync: {Sql}", sql);
                throw;
            }
        }
    }
}
