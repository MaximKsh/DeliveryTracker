using System;
using System.Threading;

namespace DeliveryTracker.Identification
{
    internal static class OnlineChecker
    {
        private static readonly object LockObj = new object();

        private const int DefaultTimeoutMillis = 60_000;

        private static int customTimeoutMillis = -1;

        private static readonly Lazy<int> TimeoutLazy;

        static OnlineChecker()
        {
            TimeoutLazy = new Lazy<int>(
                () =>
                {
                    var timeout = customTimeoutMillis;
                    return timeout == -1
                        ? DefaultTimeoutMillis
                        : timeout;
                },
                LazyThreadSafetyMode.PublicationOnly);
        }

        internal static bool Set(
            int timeoutInSeconds)
        {
            if (timeoutInSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(timeoutInSeconds)} must be greater than 0.");
            }
            
            if (customTimeoutMillis == -1)
            {
                lock (LockObj)
                {
                    if (customTimeoutMillis == -1)
                    {
                        Interlocked.Exchange(ref customTimeoutMillis, timeoutInSeconds);
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
            return user.LastActivity.AddMilliseconds(TimeoutLazy.Value) >= now;
        }

    }
}