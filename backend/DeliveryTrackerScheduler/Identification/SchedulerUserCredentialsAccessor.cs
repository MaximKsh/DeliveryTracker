using System;
using DeliveryTracker.Identification;

namespace DeliveryTrackerScheduler.Identification
{
    /// <summary>
    /// Альтернативная реализация поставщика данных о юзере, всегда возвраюащая невалидные данные
    /// </summary>
    public sealed class SchedulerUserCredentialsAccessor : IUserCredentialsAccessor
    {
        private static readonly Guid RoleId = Guid.Parse("cc3c251c-dfa1-4ff3-8116-8f24aa8176a5");

        private const string RoleName = "SchedulerRole";
        
        private readonly UserCredentials creds = new UserCredentials(
            RoleId,
            RoleName,
            Guid.Empty,
            Guid.Empty);

        public UserCredentials GetUserCredentials() => this.creds;
    }
}