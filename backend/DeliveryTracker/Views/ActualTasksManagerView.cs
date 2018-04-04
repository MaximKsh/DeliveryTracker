using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views
{
    public sealed class ActualTasksManagerView : TaskViewBase
    {
        public ActualTasksManagerView(
            int order) : base(order)
        {
        }


        public override string Name { get; } = nameof(ActualTasksManagerView);
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
                Caption = LocalizationAlias.Views.ActualTasksManagerView,
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
                "state_id = '0a79703f-4570-4a58-8509-9e598b1eefaf' " + // Waiting
                "  or state_id = '8912d18f-192a-4327-bd47-5c9963b5f2b0'" + // IntoWork
                "  or state_id = '020d7c7e-bb4e-4add-8b11-62a91471b7c8'", // Delivered
                "{0}");
        }

        protected override string ExtendSqlCount(
            string sqlCount)
        {
            return string.Format(sqlCount,
                "state_id = '0a79703f-4570-4a58-8509-9e598b1eefaf' " + // Waiting
                "  or state_id = '8912d18f-192a-4327-bd47-5c9963b5f2b0'" + // IntoWork
                "  or state_id = '020d7c7e-bb4e-4add-8b11-62a91471b7c8'"); // IntoWork
        }
    }
}