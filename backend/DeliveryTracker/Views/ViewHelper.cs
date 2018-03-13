using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace DeliveryTracker.Views
{
    public static class ViewHelper
    {
        public const string DefaultViewLimit = "10";
        
        public const string AfterParameterName = "after";

        public static void TryAddAfterParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string orderByFieldName,
            string tableName)
        {
            if (parameters.TryGetOneValue(AfterParameterName, out var value)
                && Guid.TryParse(value, out var afterId))
            {
                sb.AppendFormat(
                    "and \"{0}\" > (select \"{0}\" from \"{1}\" where \"id\" = @after_id) {2}",
                    orderByFieldName, 
                    tableName,
                    Environment.NewLine);
                command.Parameters.Add(new NpgsqlParameter("after_id", afterId));
            }
        }
        
        public static void AddGreaterThanParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            object value)
        {
            sb.AppendFormat("and {0} > @{0) {1}", name, Environment.NewLine);
            command.Parameters.Add(new NpgsqlParameter(name, value));
        }
        
        public static void TryAddEqualsParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddEqualsParameter(command, sb, name, value);
            }
        }
        
        public static void AddEqualsParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            object value)
        {
            sb.AppendFormat("and {0} = @{0) {1}", name, Environment.NewLine);
            command.Parameters.Add(new NpgsqlParameter(name, value));
        }
       
        public static void TryAddCaseInsensetiveEqualsParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddCaseInsensetiveEqualsParameter(command, sb, name, value);
            }
        }
        
        public static void AddCaseInsensetiveEqualsParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            object value)
        {
            sb.AppendFormat("and {0} = lower(@{0)) {1}", name, Environment.NewLine);
            command.Parameters.Add(new NpgsqlParameter(name, value));
        }

        public static void TryAddStartsWithParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddStartsWithParameter(command, sb, name, value);
            }
        }
        
        public static void AddStartsWithParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            object value)
        {
            sb.AppendFormat("and {0} like @{0} {1}", name, Environment.NewLine);
            command.Parameters.AddWithValue(name, value + "%");
        }
        
        public static void TryAddContainsParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddContainsParameter(command, sb, name, value);
            }
        }
        
        public static void AddContainsParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            object value)
        {
            sb.AppendFormat("and {0} like @{0} {1}", name, Environment.NewLine);
            command.Parameters.AddWithValue(name, "%" + value + "%");
        }
        
        
    }
}