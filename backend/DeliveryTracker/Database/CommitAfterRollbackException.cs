using System;

namespace DeliveryTracker.Database
{
    /// <summary>
    /// Попытка закомитить обернутую транзакцию, когда она уже запланирована на откат.
    /// </summary>
    public class CommitAfterRollbackException : Exception
    {
    }
}