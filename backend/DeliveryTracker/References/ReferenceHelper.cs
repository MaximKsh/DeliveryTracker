using System;
using System.Collections.Generic;
using System.Linq;

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
            });
            AddressColumnList = addressColumnList.AsReadOnly();
        }
        
        
        public static string GetProductColumns(string prefix = null)=>
            GetColumnsInternal(ProductColumnList, prefix);
        
        public static string GetPaymentTypeColumns(string prefix = null) =>
            GetColumnsInternal(PaymentTypeColumnList, prefix);
        
        public static string GetClientColumns(string prefix = null)=>
            GetColumnsInternal(ClientColumnList, prefix);
        
        public static string GetAddressColumns(string prefix = null)=>
            GetColumnsInternal(AddressColumnList, prefix);

        private static string GetColumnsInternal(IEnumerable<string> source, string prefix = null)
        {
            prefix = prefix ?? string.Empty;
            return string.Join("," + Environment.NewLine, source.Select(p => prefix + p));
        }
        
    }
}