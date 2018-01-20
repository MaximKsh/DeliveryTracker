using System.Data;
using DeliveryTracker.Database;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.References
{
    public static class ReferenceExtensions
    {
        #region IServiceCollection
        
        public static IServiceCollection AddDeliveryTrackerReferences(this IServiceCollection services)
        {
            return services
                .AddSingleton<IReferenceService<Product>, ProductReferenceService>()
                ;

        }
        
        #endregion
        
        #region IDataReader
        
        public static void SetReferenceBaseFields(this IDataReader reader, ReferenceEntityBase reference, ref int idx)
        {
            reference.Id = reader.GetGuid(idx++);
            reference.InstanceId = reader.GetGuid(idx++);
        }
        
        public static Product GetProduct(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetProduct(ref idx);
        }
        
        public static Product GetProduct(this IDataReader reader, ref int idx)
        {
            var product = new Product();
            reader.SetReferenceBaseFields(product, ref idx);
            product.VendorCode = reader.GetValueOrDefault<string>(idx++);
            product.Name = reader.GetValueOrDefault<string>(idx++);
            product.Description = reader.GetValueOrDefault<string>(idx++);
            product.Cost = reader.GetValueOrDefault<decimal>(idx++);
            return product;
        }
        
        public static PaymentType GetPaymentType(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetPaymentType(ref idx);
        }
        
        public static PaymentType GetPaymentType(this IDataReader reader, ref int idx)
        {
            var pt = new PaymentType();
            reader.SetReferenceBaseFields(pt, ref idx);
            pt.Name = reader.GetValueOrDefault<string>(idx++);
            return pt;
        }
        
        #endregion
    }
}