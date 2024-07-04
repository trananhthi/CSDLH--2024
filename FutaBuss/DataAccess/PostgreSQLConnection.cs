using FutaBuss.Model;
using Npgsql;

namespace FutaBuss.DataAccess
{
    public class PostgreSQLConnection
    {
        private readonly NpgsqlConnection _connection;

        public PostgreSQLConnection(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("PostgreSQL connection error: " + ex.Message, ex);
            }
        }

        public void CloseConnection()
        {
            try
            {
                _connection.Close();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("PostgreSQL closing connection error: " + ex.Message, ex);
            }
        }

        public List<Province> GetProvinces()
        {
            var provinces = new List<Province>();

            try
            {
                OpenConnection();
                string query = "SELECT code, name FROM provinces";
                using var command = new NpgsqlCommand(query, _connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string code = reader.GetString(0);
                    string name = reader.GetString(1);
                    provinces.Add(new Province(code, name));
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("PostgreSQL query error: " + ex.Message, ex);
            }
            finally
            {
                CloseConnection();
            }

            return provinces;
        }

        public Province? GetProvinceByCode(string code)
        {
            try
            {
                OpenConnection();
                string query = "SELECT code, name FROM provinces WHERE code = @code";
                using var command = new NpgsqlCommand(query, _connection);
                command.Parameters.AddWithValue("@code", code);
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string name = reader.GetString(1);
                    return new Province(code, name);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("PostgreSQL query error: " + ex.Message, ex);
            }
            finally
            {
                CloseConnection();
            }
        }


    }
}
