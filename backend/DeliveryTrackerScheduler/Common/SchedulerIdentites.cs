namespace DeliveryTrackerScheduler.Common
{
    public static class SchedulerIdentites
    {
        public static class Groups
        {
            public const string Default = nameof(Default);
        }

        public static class Jobs
        {
            public const string ExpiredInvitations = nameof(ExpiredInvitations);
            public const string ExpiredSessions = nameof(ExpiredSessions);
            public const string RouteBuilder = nameof(RouteBuilder);
        }

        public static class Triggers
        {
            public const string ExpiredInvitations = nameof(ExpiredInvitations);
            public const string ExpiredSessions = nameof(ExpiredSessions);
            public const string RouteBuilder = nameof(RouteBuilder);
        }
    }
}