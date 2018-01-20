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

        public static readonly IReadOnlyList<string> ProductColumnList;
        public static readonly IReadOnlyList<string> PaymentTypeColumnList;

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
        }
        
        public static string GetBaseColumns(string prefix = null)=>
            GetColumnsInternal(BaseColumnList, prefix);
        
        public static string GetProductColumns(string prefix = null)=>
            GetColumnsInternal(ProductColumnList, prefix);
        
        public static string GetPaymentTypeColumns(string prefix = null) =>
            GetColumnsInternal(PaymentTypeColumnList, prefix);


        private static string GetColumnsInternal(IEnumerable<string> source, string prefix = null)
        {
            prefix = prefix ?? string.Empty;
            return string.Join("," + Environment.NewLine, source.Select(p => prefix + p));
        }
        
    }
}