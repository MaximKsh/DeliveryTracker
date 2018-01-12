using System;
using Npgsql;

namespace DeliveryTracker.Database
{
    public class NpgsqlConnectionWrapper : IDisposable
    {
        private int level = 0;
        private readonly string connectionString;

        public NpgsqlConnectionWrapper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public NpgsqlConnection Connection { get; private set; }

        public NpgsqlConnectionWrapper Connect()
        {
            if (this.Connection == null)
            {
                this.Connection = new NpgsqlConnection(this.connectionString);
                this.Connection.Open();
            }
            this.level++;
            return this;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.level--;
            if (this.level == 0)
            {
                this.Connection.Dispose();
                this.Connection = null;
            }
        }
    }
}