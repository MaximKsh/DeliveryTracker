using System.Data;
using DeliveryTracker.Database;
using DeliveryTracker.Geopositioning;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.References
{
    public static class ReferenceExtensions
    {
        #region IServiceCollection
        
        public static IServiceCollection AddDeliveryTrackerReferences(this IServiceCollection services)
        {
            return services
                .AddSingleton<IReferenceFacade, ReferenceFacade>()
                .AddSingleton<IReferenceService, PaymentTypeReferenceService>()
                .AddSingleton<IReferenceService, ProductReferenceService>()
                .AddSingleton<IReferenceService, ClientReferenceService>()
                ;

        }
        
        #endregion
        
        #region IDataReader
        
        public static void SetReferenceBaseFields(
            this IDataReader reader,
            ReferenceEntityBase reference, 
            ref int idx)
        {
            reference.Id = reader.GetGuid(idx++);
            reference.InstanceId = reader.GetGuid(idx++);
        }
        
        public static void SetCollectionReferenceBaseFields(
            this IDataReader reader, 
            ReferenceCollectionBase reference,
            ref int idx)
        {
            reference.ParentId = reader.GetGuid(idx++);
        }
        
        public static Client GetClient(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetClient(ref idx);
        }
        
        public static Client GetClient(this IDataReader reader, ref int idx)
        {
            var client = new Client();
            reader.SetReferenceBaseFields(client, ref idx);
            client.Surname = reader.GetValueOrDefault<string>(idx++);
            client.Name = reader.GetValueOrDefault<string>(idx++);
            client.Patronymic = reader.GetValueOrDefault<string>(idx++);
            client.PhoneNumber = reader.GetValueOrDefault<string>(idx++);
            return client;
        }
        
        public static Address GetAddress(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetAddress(ref idx);
        }
        
        public static Address GetAddress(this IDataReader reader, ref int idx)
        {
            var address = new Address();
            reader.SetReferenceBaseFields(address, ref idx);
            reader.SetCollectionReferenceBaseFields(address, ref idx);
            address.RawAddress = reader.GetValueOrDefault<string>(idx++);
            var lon = reader.GetValueOrDefault<double?>(idx++);
            var lat = reader.GetValueOrDefault<double?>(idx++);
            if (lat.HasValue
                && lon.HasValue)
            {
                address.Geoposition = new Geoposition
                {
                    Longitude = lon.Value,
                    Latitude = lat.Value,
                };
            }
            return address;
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