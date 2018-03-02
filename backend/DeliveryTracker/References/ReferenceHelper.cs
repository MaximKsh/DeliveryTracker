using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Database;

namespace DeliveryTracker.References
{
    public static class ReferenceHelper
    {
        public static readonly IReadOnlyList<string> BaseColumnList = new List<string>
        {
            "id", 
            "instance_id", 
        }.AsReadOnly();
        
        public static readonly IReadOnlyList<string> CollectionColumnList = new List<string>
        {
            "parent_id", 
        }.AsReadOnly();

        public static readonly IReadOnlyList<string> ProductColumnList;
        public static readonly IReadOnlyList<string> PaymentTypeColumnList;
        public static readonly IReadOnlyList<string> ClientColumnList;
        public static readonly IReadOnlyList<string> AddressColumnList;
        public static readonly IReadOnlyList<string> WarehouseColumnList;

        static ReferenceHelper()
        {
            var productColumnList = new List<string>();
            productColumnList.AddRange(BaseColumnList);
            productColumnList.AddRange(new []
            {
                "vendor_code",
                "name",
                "description",
                "cost"
            });
            ProductColumnList = productColumnList.AsReadOnly();
            
            var paymentTypeColumnList = new List<string>();
            paymentTypeColumnList.AddRange(BaseColumnList);
            paymentTypeColumnList.AddRange(new []
            {
                "name",
            });
            PaymentTypeColumnList = paymentTypeColumnList.AsReadOnly();
            
            var clientColumnList = new List<string>();
            clientColumnList.AddRange(BaseColumnList);
            clientColumnList.AddRange(new []
            {
                "surname",
                "name",
                "patronymic",
                "phone_number"
            });
            ClientColumnList = clientColumnList.AsReadOnly();
            
            var addressColumnList = new List<string>();
            addressColumnList.AddRange(BaseColumnList);
            addressColumnList.AddRange(CollectionColumnList);
            addressColumnList.AddRange(new []
            {
                "raw_address",
                "ST_X(geoposition::geometry)",
                "ST_Y(geoposition::geometry)",
            });
            AddressColumnList = addressColumnList.AsReadOnly();
            
            var warehouseColumnList = new List<string>();
            warehouseColumnList.AddRange(BaseColumnList);
            warehouseColumnList.AddRange(new []
            {
                "name",
                "raw_address",
                "ST_X(geoposition::geometry)",
                "ST_Y(geoposition::geometry)",
            });
            WarehouseColumnList = warehouseColumnList.AsReadOnly();
        }
        
        
        public static string GetProductColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(ProductColumnList, prefix);
        
        
        public static string GetPaymentTypeColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(PaymentTypeColumnList, prefix);
        
        
        public static string GetClientColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(ClientColumnList, prefix);
        
        
        public static string GetAddressColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(AddressColumnList, prefix);
        
        
        public static string GetWarehouseColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(WarehouseColumnList, prefix);
    }
}