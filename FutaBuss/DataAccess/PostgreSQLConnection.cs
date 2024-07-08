//using FutaBuss.Model;
//using Npgsql;

//namespace FutaBuss.DataAccess
//{
//    public class PostgreSQLConnection
//    {
//        private readonly NpgsqlConnection _connection;

//        public PostgreSQLConnection(string connectionString)
//        {
//            _connection = new NpgsqlConnection(connectionString);
//        }

//        public void OpenConnection()
//        {
//            try
//            {
//                _connection.Open();
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException("PostgreSQL connection error: " + ex.Message, ex);
//            }
//        }

//        public void CloseConnection()
//        {
//            try
//            {
//                _connection.Close();
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException("PostgreSQL closing connection error: " + ex.Message, ex);
//            }
//        }

//        public List<Province> GetProvinces()
//        {
//            var provinces = new List<Province>();

//            try
//            {
//                OpenConnection();
//                string query = "SELECT code, name FROM provinces";
//                using var command = new NpgsqlCommand(query, _connection);
//                using var reader = command.ExecuteReader();

//                while (reader.Read())
//                {
//                    string code = reader.GetString(0);
//                    string name = reader.GetString(1);
//                    provinces.Add(new Province(code, name));
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException("PostgreSQL query error: " + ex.Message, ex);
//            }
//            finally
//            {
//                CloseConnection();
//            }

//            return provinces;
//        }

//        public Province? GetProvinceByCode(string code)
//        {
//            try
//            {
//                OpenConnection();
//                string query = "SELECT code, name FROM provinces WHERE code = @code";
//                using var command = new NpgsqlCommand(query, _connection);
//                command.Parameters.AddWithValue("@code", code);
//                using var reader = command.ExecuteReader();

//                if (reader.Read())
//                {
//                    string name = reader.GetString(1);
//                    return new Province(code, name);
//                }
//                else
//                {
//                    return null;
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException("PostgreSQL query error: " + ex.Message, ex);
//            }
//            finally
//            {
//                CloseConnection();
//            }
//        }

//        public int? AddNewUser(User user)
//        {
//            try
//            {
//                OpenConnection();
//                string query = "INSERT INTO users (fullname, phone, email) VALUES (@fullname, @phone, @email) returning id";
//                using var command = new NpgsqlCommand(query, _connection);
//                command.Parameters.AddWithValue("@fullname", user.FullName);
//                command.Parameters.AddWithValue("@phone", user.PhoneNumber);
//                command.Parameters.AddWithValue("@email", user.Email);
//                int? result = (int?)command.ExecuteScalar();
//                return result;
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException("PostgreSQL query error: " + ex.Message, ex);
//            }
//            finally
//            {
//                CloseConnection();
//            }
//        }
//    }
//}



using FutaBuss.Model;
using Npgsql;

namespace FutaBuss.DataAccess
{
    public class PostgreSQLConnection
    {
        private static readonly Lazy<PostgreSQLConnection> _instance = new Lazy<PostgreSQLConnection>(() => new PostgreSQLConnection());
        private readonly NpgsqlConnection _connection;

        // Connection string is hardcoded here
        private const string ConnectionString = "Host=dpg-cq12053v2p9s73cjijm0-a.singapore-postgres.render.com;Username=root;Password=vTwWs92lObTZrhI9IFcJGXJxZCdzeBas;Database=mds_postpresql";

        // Private constructor to prevent instantiation from outside
        private PostgreSQLConnection()
        {
            _connection = new NpgsqlConnection(ConnectionString);
        }

        public static PostgreSQLConnection Instance => _instance.Value;

        public async Task OpenConnectionAsync()
        {
            try
            {
                await _connection.OpenAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("PostgreSQL connection error: " + ex.Message, ex);
            }
        }

        public async Task CloseConnectionAsync()
        {
            try
            {
                await _connection.CloseAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("PostgreSQL closing connection error: " + ex.Message, ex);
            }
        }

        public async Task<List<Province>> GetProvincesAsync()
        {
            var provinces = new List<Province>();

            try
            {
                await OpenConnectionAsync();
                await using (var command = new NpgsqlCommand("SELECT code, name FROM provinces", _connection))
                await using (var reader = await command.ExecuteReaderAsync())

                {
                    while (await reader.ReadAsync())
                    {
                        string code = reader.GetString(0);
                        string name = reader.GetString(1);
                        provinces.Add(new Province(code, name));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("PostgreSQL query error: " + ex.Message, ex);
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return provinces;
        }

        public async Task<Province?> GetProvinceByCodeAsync(string code)
        {
            try
            {
                await OpenConnectionAsync();
                await using (var command = new NpgsqlCommand("SELECT code, name FROM provinces WHERE code = @code", _connection))
                {
                    command.Parameters.AddWithValue("@code", code);
                    using var reader = command.ExecuteReader();
                    if (await reader.ReadAsync())
                    {
                        string name = reader.GetString(1);
                        return new Province(code, name);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("PostgreSQL query error: " + ex.Message, ex);
            }
            finally
            {
                await CloseConnectionAsync();
            }
        }

        public async Task AddNewUserAsync(User user)
        {
            try
            {
                await OpenConnectionAsync();
                await using (var command = new NpgsqlCommand("INSERT INTO users (id, fullname, phone, email) VALUES (@id ,@fullname, @phone, @email)", _connection))
                {
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@fullname", user.FullName);
                    command.Parameters.AddWithValue("@phone", user.PhoneNumber);
                    command.Parameters.AddWithValue("@email", user.Email);
                    await command.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException("PostgreSQL query error: " + ex.Message, ex);
            }
            finally
            {
                await CloseConnectionAsync();
            }
        }
    }
}
