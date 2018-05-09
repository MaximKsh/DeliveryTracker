using System;
using DeliveryTracker.Identification;

namespace DeliveryTrackerScheduler.Identification
{
    /// <summary>
    /// Альтернативная реализация поставщика данных о юзере, всегда возвраюащая невалидные данные
    /// </summary>
    public sealed class SchedulerUserCredentialsAccessor : IUserCredentialsAccessor
    {
        private readonly UserCredentials creds = new UserCredentials(
            Guid.Empty,
            string.Empty,
            Guid.Empty,
            Guid.Empty);

        public UserCredentials GetUserCredentials() => this.creds;
    }
}