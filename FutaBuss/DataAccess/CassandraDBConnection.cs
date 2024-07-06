using Cassandra;
using System.IO;
using System.Windows;

namespace FutaBuss.DataAccess
{
    public class CassandraDBConnection
    {
        private static readonly Lazy<CassandraDBConnection> _instance = new Lazy<CassandraDBConnection>(() => new CassandraDBConnection());
        private readonly ISession _session;

        // Cassandra Astra connection details
        private const string clientId = "NurzkWTRQhNjHRWeddORtjxd";
        private const string clientSecret = "Zl60ab8yFucPW_Z5.3sO3Cjx4DMlZGJL32Cx5SnyWl_4+GfAkMEFZ3UF44v1ZeRb_hkqzTn,CdSOh4USF2,-tErnBSuiFvTk9JdtOG8YedGArFUn1Ia_uzqSqgkKnA_n";
        //private const string SecureConnectBundlePath = "C:\\Users\\Admin\\Desktop\\NoSQL\\secure-connect-futabus.zip";
        private static readonly string SecureConnectBundlePath = Path.Combine("..\\..\\..", "Bundle", "secure-connect-futabus.zip");

        private const string Keyspace = "futabus";

        // Private constructor to prevent instantiation from outside
        private CassandraDBConnection()
        {
            try
            {
                // Create cluster and session
                var cluster = Cluster.Builder()
                    .WithCloudSecureConnectionBundle(SecureConnectBundlePath)
                    .WithCredentials(clientId, clientSecret)
                    .Build();

                _session = cluster.Connect(Keyspace);

            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cassandra connection error: " + ex.Message, ex);
            }
        }

        public static CassandraDBConnection Instance => _instance.Value;

        public ISession GetSession()
        {
            return _session;
        }
    }
}
