using System;
using System.Threading;

namespace DeliveryTracker.Identification
{
    internal static class OnlineChecker
    {
        private static readonly object LockObj = new object();

        private const int DefaultTimeoutInSeconds = 60;

        private static int customTimeoutInSeconds = -1;

        private static readonly Lazy<int> TimeoutLazy;

        static OnlineChecker()
        {
            TimeoutLazy = new Lazy<int>(
                () =>
                {
                    var timeout = customTimeoutInSeconds;
                    return timeout == -1
                        ? DefaultTimeoutInSeconds
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
            
            if (customTimeoutInSeconds == -1)
            {
                lock (LockObj)
                {
                    if (customTimeoutInSeconds == -1)
                    {
                        Interlocked.Exchange(ref customTimeoutInSeconds, timeoutInSeconds);
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