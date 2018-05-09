using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views.Tasks
{
    public sealed class ActualTasksPerformerView : TaskViewBase
    {
        public ActualTasksPerformerView(
            int order,
            ITaskService taskService) : base(order, taskService)
        {
        }


        public override string Name { get; } = nameof(ActualTasksPerformerView);
        public override IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.PerformerRole
        }.AsReadOnly();

        protected override ViewDigest ViewDigestFactory(
            long count)
        {
            return new ViewDigest
            {
                Caption = LocalizationAlias.Views.ActualTasksPerformerView,
                Count = count,
                EntityType = nameof(TaskInfo),
                Order = this.Order,
            };
        }

        protected override string ExtendSqlGet(
            string sqlGet)
        {
            return string.Format(sqlGet,
                "performer_id = @user_id " + 
                "and (state_id = '0a79703f-4570-4a58-8509-9e598b1eefaf' " + // Waiting
                "  or state_id = '8912d18f-192a-4327-bd47-5c9963b5f2b0')", // IntoWork
                "{0}");
        }

        protected override string ExtendSqlCount(
            string sqlCount)
        {
            return string.Format(sqlCount,
                "performer_id = @user_id " + 
                "and (state_id = '0a79703f-4570-4a58-8509-9e598b1eefaf' " + // Waiting
                "  or state_id = '8912d18f-192a-4327-bd47-5c9963b5f2b0')"); // IntoWork
        }

    }
}