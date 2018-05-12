using System;
using System.Threading;

namespace DeliveryTracker.Identification
{
    internal static class OnlineChecker
    {
        private static readonly object LockObj = new object();

        private const int DefaultTimeoutMinutes = 1;

        private static int customTimeoutMinutes = -1;

        private static readonly Lazy<int> TimeoutLazy;

        static OnlineChecker()
        {
            TimeoutLazy = new Lazy<int>(
                () =>
                {
                    var timeout = customTimeoutMinutes;
                    return timeout == -1
                        ? DefaultTimeoutMinutes
                        : timeout;
                },
                LazyThreadSafetyMode.PublicationOnly);
        }

        internal static bool Set(
            int timeoutInMinutes)
        {
            if (timeoutInMinutes <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(timeoutInMinutes)} must be greater than 0.");
            }
            
            if (customTimeoutMinutes == -1)
            {
                lock (LockObj)
                {
                    if (customTimeoutMinutes == -1)
                    {
                        Interlocked.Exchange(ref customTimeoutMinutes, timeoutInMinutes);
                        return true;
                    }
                }
            }

            return false;
        }
        

        internal static bool IsOnline(
            User user)
        {
            var now = DateTime.UtcNow;
            return user.LastActivity.AddMinutes(TimeoutLazy.Value) >= now;
        }

    }
}