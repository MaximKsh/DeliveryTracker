using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views
{
    public sealed class RevokedTasksPerformerView : TaskViewBase
    {
        public RevokedTasksPerformerView(
            int order) : base(order)
        {
        }

        public override string Name { get; } = nameof(RevokedTasksPerformerView);
        public override IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.PerformerRole,
        }.AsReadOnly();

        protected override ViewDigest ViewDigestFactory(
            long count)
        {
            return new ViewDigest
            {
                Caption = LocalizationAlias.Views.RevokedTasksPerformerView,
                Count = count,
                EntityType = nameof(TaskInfo),
                Order = this.Order,
                IconName = "Я не знаю"
            };
        }

        protected override string ExtendSqlGet(
            string sqlGet)
        {
            return string.Format(sqlGet,
                "state_id = 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9' " +
                "and performer_id = @user_id", // Revoked
                "{0}");
        }

        protected override string ExtendSqlCount(
            string sqlCount)
        {
            return string.Format(sqlCount,
                "state_id = 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9' " +
                "and performer_id = @user_id" // Revoked
            ); 
        }
    }
}