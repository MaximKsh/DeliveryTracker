using DeliveryTracker.Common;
using DeliveryTracker.References;

namespace DeliveryTrackerWeb.Tests.Integration
{
    public static class Extensions
    {
        public static ReferencePackage Pack(
            this ReferenceEntryBase obj)
        {
            return new ReferencePackage
            {
                Entry = obj
            };
        }
    }
}