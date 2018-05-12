using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Npgsql;

namespace DeliveryTracker.Views
{
    public static class ViewHelper
    {
        #region constants
        
        public const string DefaultViewLimit = "100";
        
        public const string AfterParameterName = "after";

        #endregion
        
        #region after (paging)
        
        public static void TryAddAfterParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string orderByFieldName,
            string tableName,
            bool desc = false)
        {
            if (parameters.TryGetOneValue(AfterParameterName, out var value)
                && Guid.TryParse(value, out var afterId))
            {
                sb.AppendFormat(
                    "and \"{0}\" {3} (select \"{0}\" from \"{1}\" where \"id\" = @after_id) {2}",
                    orderByFieldName, 
                    tableName,
                    Environment.NewLine,
                    desc ? "<" : ">");
                command.Parameters.Add(new NpgsqlParameter("after_id", afterId));
            }
        }
        
        #endregion
        
        #region equals
        
        public static void TryAddEqualsParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName = null)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddEqualsParameter(command, sb, name, columnName, value);
            }
        }
        
        public static void AddEqualsParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName,
            object value)
        {
            if (columnName is null)
            {
                columnName = name;
            }
            sb.AppendFormat("and {0} = @{1} {2}", columnName, name, Environment.NewLine);
            command.Parameters.Add(new NpgsqlParameter(name, value));
        }
        
        public static void TryAddEqualsParameter<T>(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName = null)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddEqualsParameter<T>(command, sb, name, columnName, value);
            }
        }
        
        public static void AddEqualsParameter<T>(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName,
            object value)
        {
            T result;
            var converter = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                if (converter.CanConvertFrom(value.GetType()))
                {
                    result = (T) converter.ConvertFrom(value);
                }
                else
                {
                    result = (T) converter.ConvertFromString(value.ToString());
                }
            }
            catch (Exception)
            {
                return;
            }
            
            if (columnName is null)
            {
                columnName = name;
            }
            sb.AppendFormat("and {0} = @{1} {2}", columnName, name, Environment.NewLine);
            command.Parameters.Add(new NpgsqlParameter(name, result));
        }
        
       
        public static void TryAddCaseInsensetiveEqualsParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName = null)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddCaseInsensetiveEqualsParameter(command, sb, name, columnName, value);
            }
        }
        
        public static void AddCaseInsensetiveEqualsParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName,
            object value)
        {
            if (columnName is null)
            {
                columnName = name;
            }
            
            sb.AppendFormat("and {0} = lower(@{1}) {2}",columnName, name, Environment.NewLine);
            command.Parameters.Add(new NpgsqlParameter(name, value));
        }
        
        #endregion
        
        #region startswith parameter

        public static void TryAddStartsWithParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName = null)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddStartsWithParameter(command, sb, name, columnName ?? name, value);
            }
        }
        
        public static void AddStartsWithParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName,
            object value)
        {
            if (columnName is null)
            {
                columnName = name;
            }
            
            sb.AppendFormat("and {0} like @{1} {2}", columnName, name, Environment.NewLine);
            command.Parameters.AddWithValue(name, $"{value}%");
        }
        
        public static void TryAddCaseInsensetiveStartsWithParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName = null)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddStartsWithParameter(command, sb, name, columnName ?? name, value);
            }
        }
        
        public static void AddCaseInsensetiveStartsWithParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName,
            object value)
        {
            if (columnName is null)
            {
                columnName = name;
            }
            
            sb.AppendFormat("and {0} like @{1} {2}", columnName, name, Environment.NewLine);
            command.Parameters.AddWithValue(name, $"{value}%");
        }
        
        #endregion
        
        #region contains (case sensetive/insensetive)
        
        public static void TryAddContainsParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName = null)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddContainsParameter(command, sb, name, columnName ?? name, value);
            }
        }
        
        public static void AddContainsParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName,
            object value)
        {
            if (columnName is null)
            {
                columnName = name;
            }
            sb.AppendFormat("and {0} like @{1} {2}", columnName, name, Environment.NewLine);
            command.Parameters.AddWithValue(name, $"%{value}%");
        }
        
        public static void TryAddCaseInsensetiveContainsParameter(
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName = null)
        {
            if (parameters.TryGetOneValue(name, out var value))
            {
                AddCaseInsensetiveContainsParameter(command, sb, name, columnName ?? name, value);
            }
        }
        
        public static void AddCaseInsensetiveContainsParameter(
            NpgsqlCommand command,
            StringBuilder sb,
            string name,
            string columnName,
            object value)
        {
            if (columnName is null)
            {
                columnName = name;
            }
            sb.AppendFormat("and lower({0}) like @{1} {2}", columnName, name, Environment.NewLine);
            command.Parameters.AddWithValue(name, $"%{value.ToString().ToLowerInvariant()}%");
        }
        
        #endregion
        
    }
}