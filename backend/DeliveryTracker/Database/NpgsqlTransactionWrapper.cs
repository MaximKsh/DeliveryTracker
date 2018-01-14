using System;
using Npgsql;

namespace DeliveryTracker.Database
{
    /// <summary>
    /// Обертка над Npgsql транзакцией,
    /// позволяющая использовать одну транзакцию в вложенных using блоках.
    /// </summary>
    public class NpgsqlTransactionWrapper : IDisposable
    {
        #region fields

        private readonly bool autoCommit;
        
        private int level = 0;

        #endregion
        
        #region constructor

        public NpgsqlTransactionWrapper(NpgsqlTransaction transaction, bool autoCommit = false)
        {
            this.Transaction = transaction;
            this.autoCommit = autoCommit;
        }
        
        #endregion

        #region properties
        
        /// <summary>
        /// Npgsql транзакция.
        /// </summary>
        public NpgsqlTransaction Transaction { get; private set; }

        /// <summary>
        /// Транзакция завершена.
        /// </summary>
        public bool Disposed => this.Transaction == null;

        /// <summary>
        /// При вызове Dispose на верхнем уровне транзакция будет принята.
        /// </summary>
        public bool DefferedCommit { get; private set; } = false;

        /// <summary>
        /// При вызове Dispose на верхнем уровне транзакция будет отклонена.
        /// </summary>
        public bool DefferedRollback { get; private set; } = false;

        #endregion
        
        #region public methods
        
        /// <summary>
        /// Увеличить уровень вложенности.
        /// Не использовать вне NpgsqlConnectionWrapper
        /// </summary>
        public void IncrementLevel() => this.level++;

        /// <summary>
        /// Запланировать принятие транзакции на верхнем уровне вложенности.
        /// Транзакция будет принята в Dispose верхнего блока using.
        /// </summary>
        /// <exception cref="CommitAfterRollbackException"></exception>
        public void Commit()
        {
            if (this.DefferedRollback)
            {
                throw new CommitAfterRollbackException();
            }
            
            if (!this.DefferedCommit)
            {
                this.DefferedCommit = true;
            }
        }
        
        /// <summary>
        /// Запланировать откат транзакции на верхнем уровне вложенности.
        /// Транзакция будет отменена в Dispose верхнего блока using.
        /// </summary>
        /// <exception cref="CommitAfterRollbackException"></exception>
        public void Rollback()
        {
            if (!this.DefferedRollback)
            {
                this.DefferedRollback = true;
            }
        }
        
        #endregion
        
        #region IDisposable
        
        /// <inheritdoc />
        public void Dispose()
        {
            this.level--;
            if (this.level != 0)
            {
                return;
            }
            if (this.DefferedRollback)
            {
                this.Transaction.Rollback();
            }
            else if (this.DefferedCommit || this.autoCommit)
            {
                this.Transaction.Commit();
            }
            else if (!this.autoCommit)
            {
                this.Transaction.Rollback();
            }
                
            this.Transaction.Dispose();
            this.Transaction = null;
        }
        
        #endregion
    }
}