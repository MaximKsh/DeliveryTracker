using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views.Tasks
{
    public sealed class RevokedTasksManagerView: TaskViewBase
    {
        public RevokedTasksManagerView(
            int order,
            ITaskService taskService) : base(order, taskService)
        {
        }


        public override string Name { get; } = nameof(RevokedTasksManagerView);
        public override IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.CreatorRole,
            DefaultRoles.ManagerRole,
        }.AsReadOnly();

        protected override ViewDigest ViewDigestFactory(
            long count)
        {
            return new ViewDigest
            {
                Caption = LocalizationAlias.Views.RevokedTasksManagerView,
                Count = count,
                EntityType = nameof(TaskInfo),
                Order = this.Order,
            };
        }

        protected override string ExtendSqlGet(
            string sqlGet)
        {
            return string.Format(sqlGet,
                "state_id = 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9'", // Revoked
                "{0}");
        }

        protected override string ExtendSqlCount(
            string sqlCount)
        {
            return string.Format(sqlCount,
                "state_id = 'd4595da3-6a5f-4455-b975-7637ea429cb5'" // Revoked
            ); 
        }
    }
}