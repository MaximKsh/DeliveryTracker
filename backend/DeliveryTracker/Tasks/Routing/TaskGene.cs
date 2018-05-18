using System;

namespace DeliveryTracker.Tasks.Routing
{
    public struct TaskGene
    {
        /// <summary>
        /// Идентификатор задания.
        /// </summary>
        public Guid TaskId;

        /// <summary>
        /// Исполнитель задания.
        /// </summary>
        public Guid? PerformerId;

        /// <summary>
        /// Время открытия окна в секундах от начала суток.
        /// </summary>
        public int TimeWindowStart;

        /// <summary>
        /// Время закрытия окна в секундах от начала суток
        /// </summary>
        public int TimeWindowEnd;

        public double XPosition;

        public double YPosition;
    }
}