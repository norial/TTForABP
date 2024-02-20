using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace TTForABP.Models
{
    public class DbManager
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DbManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public void ExecuteStoredProcedure(string storedProcedureName, Action<DbCommand> addParameters)
        {
            using (DbConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = storedProcedureName;
                    cmd.CommandType = CommandType.StoredProcedure;

                    addParameters?.Invoke(cmd);

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public string GetExperimentValueByDeviceToken(string key, string deviceToken)
        {
            string result = null;

            ExecuteStoredProcedure("GetExperimentByDeviceToken", cmd =>
            {
                var parameterKey = cmd.CreateParameter();
                parameterKey.ParameterName = "@Key";
                parameterKey.Value = key;
                cmd.Parameters.Add(parameterKey);

                var parameterDeviceToken = cmd.CreateParameter();
                parameterDeviceToken.ParameterName = "@DeviceToken";
                parameterDeviceToken.Value = deviceToken;
                cmd.Parameters.Add(parameterDeviceToken);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = reader["Value"].ToString();
                    }
                }
            });

            return result;
        }
    }

        public class AppDbContext : DbContext
        {
            public DbSet<Experiment> Experiments { get; set; }

            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        }
}
