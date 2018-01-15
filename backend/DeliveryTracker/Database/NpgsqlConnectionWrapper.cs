using System;
using System.Data;
using Npgsql;

namespace DeliveryTracker.Database
{
    public class NpgsqlConnectionWrapper : IDisposable
    {
        #region fields
        
        private int level = 0;
        private readonly string connectionString;

        #endregion
        
        #region constuctor
        
        public NpgsqlConnectionWrapper(string connectionString)
        {
            this.connectionString = connectionString;
        }
        
        #endregion

        #region properties
        
        public NpgsqlConnection Connection { get; private set; }

        public NpgsqlTransactionWrapper TransactionWrapper { get; private set; }
        
        #endregion
        
        #region public methods
        
        /// <summary>
        /// Открыть соединение с учетом вложенности.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Начать транзакцию или получить существующую
        /// </summary>
        /// <returns></returns>
        public NpgsqlTransactionWrapper BeginTransaction()
        {
            if (this.TransactionWrapper == null
                || this.TransactionWrapper.Disposed)
            {
                var transact = this.Connection.BeginTransaction();
                this.TransactionWrapper = new NpgsqlTransactionWrapper(transact);
            }
            this.TransactionWrapper.IncrementLevel();
            return this.TransactionWrapper;
        }
        
        /// <summary>
        /// Начать транзацию или получить существующую
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public NpgsqlTransactionWrapper BeginTransaction(IsolationLevel isolationLevel)
        {
            if (this.TransactionWrapper == null
                || this.TransactionWrapper.Disposed)
            {
                var transact = this.Connection.BeginTransaction(isolationLevel);
                this.TransactionWrapper = new NpgsqlTransactionWrapper(transact);
            }
            this.TransactionWrapper.IncrementLevel();
            return this.TransactionWrapper;
        }

        /// <summary>
        /// Создать Npgsql команду.
        /// </summary>
        /// <returns></returns>
        public NpgsqlCommand CreateCommand()
        {
            return this.Connection.CreateCommand();
        }
        
        #endregion
        
        #region IDisposable
        
        /// <inheritdoc />
        public void Dispose()
        {
            this.level--;
            if (this.level == 0)
            {
                this.Connection.Dispose();
            }
        }
        
        #endregion
        
    }
}