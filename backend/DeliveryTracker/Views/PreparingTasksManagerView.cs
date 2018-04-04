using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views
{
    public sealed class PreparingTasksManagerView: TaskViewBase
    {
        public PreparingTasksManagerView(
            int order) : base(order)
        {
        }


        public override string Name { get; } = nameof(PreparingTasksManagerView);
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
                Caption = LocalizationAlias.Views.PreparingTasksManagerView,
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
                "state_id = '8c9c1011-f7c1-4cef-902f-4925f5e83f4a'", // Preparing
                "{0}");
        }

        protected override string ExtendSqlCount(
            string sqlCount)
        {
            return string.Format(sqlCount,
                "state_id = '8c9c1011-f7c1-4cef-902f-4925f5e83f4a'" // Preparing
            ); 
        }
    }
}