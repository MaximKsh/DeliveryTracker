﻿using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views.Tasks
{
    public sealed class QueueTasksPerformerView: TaskViewBase
    {
        public QueueTasksPerformerView(
            int order,
            ITaskService taskService) : base(order, taskService)
        {
        }

        public override string Name { get; } = nameof(QueueTasksPerformerView);
        public override IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.PerformerRole,
        }.AsReadOnly();

        protected override ViewDigest ViewDigestFactory(
            long count)
        {
            return new ViewDigest
            {
                Caption = LocalizationAlias.Views.QueueTasksPerformerView,
                Count = count,
                EntityType = nameof(TaskInfo),
                Order = this.Order,
            };
        }

        protected override string ExtendSqlGet(
            string sqlGet)
        {
            return string.Format(sqlGet,
                "state_id = 'd4595da3-6a5f-4455-b975-7637ea429cb5' " +
                "and performer_id = @user_id", // Queue
                "{0}");
        }

        protected override string ExtendSqlCount(
            string sqlCount)
        {
            return string.Format(sqlCount,
                "state_id = 'd4595da3-6a5f-4455-b975-7637ea429cb5' " +
                "and performer_id = @user_id" // Queue
            ); 
        }
    }
}