using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views
{
    public sealed class QueueTasksManagerView: TaskViewBase
    {
        public QueueTasksManagerView(
            int order) : base(order)
        {
        }


        public override string Name { get; } = nameof(QueueTasksManagerView);
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
                Caption = LocalizationAlias.Views.QueueTasksManagerView,
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
                "state_id = 'd4595da3-6a5f-4455-b975-7637ea429cb5'", // Queue
                "{0}");
        }

        protected override string ExtendSqlCount(
            string sqlCount)
        {
            return string.Format(sqlCount,
                "state_id = 'd4595da3-6a5f-4455-b975-7637ea429cb5'" // Queue
            ); 
        }
    }
}