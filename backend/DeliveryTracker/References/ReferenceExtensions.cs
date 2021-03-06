﻿using System.Data;
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
                .AddSingleton<IReferenceService<PaymentType>, PaymentTypeReferenceService>()
                .AddSingleton<IReferenceService>(x => x.GetService<IReferenceService<PaymentType>>())
                .AddSingleton<IReferenceService<Product>, ProductReferenceService>()
                .AddSingleton<IReferenceService>(x => x.GetService<IReferenceService<Product>>())
                .AddSingleton<IReferenceService<Client>, ClientReferenceService>()
                .AddSingleton<IReferenceService>(x => x.GetService<IReferenceService<Client>>())
                .AddSingleton<IReferenceService<Warehouse>, WarehouseReferenceService>()
                .AddSingleton<IReferenceService>(x => x.GetService<IReferenceService<Warehouse>>())
                
                .AddSingleton<ICollectionReferenceService<ClientAddress>, ClientAddressReferenceService>()
                .AddSingleton<ICollectionReferenceService>(x => x.GetService<ICollectionReferenceService<ClientAddress>>())
                ;

        }
        
        #endregion
        
        #region IDataReader
        
        public static void SetReferenceBaseFields(
            this IDataReader reader,
            ReferenceEntryBase reference, 
            ref int idx)
        {
            reference.Id = reader.GetGuid(idx++);
            reference.InstanceId = reader.GetGuid(idx++);
            reference.Deleted = reader.GetBoolean(idx++);
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
        
        public static ClientAddress GetAddress(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetAddress(ref idx);
        }
        
        public static ClientAddress GetAddress(this IDataReader reader, ref int idx)
        {
            var address = new ClientAddress();
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
        
        public static Warehouse GetWarehouse(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetWarehouse(ref idx);
        }
        
        public static Warehouse GetWarehouse(this IDataReader reader, ref int idx)
        {
            var warehouse = new Warehouse();
            reader.SetReferenceBaseFields(warehouse, ref idx);
            warehouse.Name = reader.GetValueOrDefault<string>(idx++);
            warehouse.RawAddress = reader.GetValueOrDefault<string>(idx++);
            var lon = reader.GetValueOrDefault<double?>(idx++);
            var lat = reader.GetValueOrDefault<double?>(idx++);
            if (lat.HasValue
                && lon.HasValue)
            {
                warehouse.Geoposition = new Geoposition
                {
                    Longitude = lon.Value,
                    Latitude = lat.Value,
                };
            }
            return warehouse;
        }
        
        #endregion
    }
}