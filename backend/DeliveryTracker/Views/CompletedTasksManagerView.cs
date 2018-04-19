using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views
{
    public sealed class CompleteTasksManager: TaskViewBase
    {
        public CompleteTasksManager(
            int order,
            ITaskService taskService) : base(order, taskService)
        {
        }


        public override string Name { get; } = nameof(CompleteTasksManager);
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
                Caption = LocalizationAlias.Views.CompletedTasksManagerView,
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
                "state_id = 'd91856f9-d1bf-4fad-a46e-c3baafabf762'", // Completed
                "{0}");
        }

        protected override string ExtendSqlCount(
            string sqlCount)
        {
            return string.Format(sqlCount,
                "state_id = 'd91856f9-d1bf-4fad-a46e-c3baafabf762'" // Completed
                ); 
        }
    }
}