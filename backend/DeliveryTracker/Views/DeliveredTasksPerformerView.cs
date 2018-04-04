using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views
{
    public sealed class DeliveredTasksPerformerView: TaskViewBase
    {
        public DeliveredTasksPerformerView(
            int order) : base(order)
        {
        }

        public override string Name { get; } = nameof(DeliveredTasksPerformerView);
        public override IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.PerformerRole,
        }.AsReadOnly();

        protected override ViewDigest ViewDigestFactory(
            long count)
        {
            return new ViewDigest
            {
                Caption = LocalizationAlias.Views.DeliveredTasksPerformerView,
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
                "state_id = '020d7c7e-bb4e-4add-8b11-62a91471b7c8' " +
                "and performer_id = @user_id", // Delivered
                "{0}");
        }

        protected override string ExtendSqlCount(
            string sqlCount)
        {
            return string.Format(sqlCount,
                "state_id = '020d7c7e-bb4e-4add-8b11-62a91471b7c8' " +
                "and performer_id = @user_id" // Delivered
            ); 
        }
    }
}