using System.Collections.Generic;
using DeliveryTracker.Database;

namespace DeliveryTracker.Tasks
{
    public static class TaskHelper
    {
        public static readonly IReadOnlyList<string> TaskStateTransitionColumns = new List<string>
        {
            "id", 
            "role", 
            "initial_state",
            "final_state",
            "button_caption",
        }.AsReadOnly();
        
        public static readonly IReadOnlyList<string> TaskColumns = new List<string>
        {
            "id", 
            "instance_id", 
            "state_id",
            "author_id",
            "performer_id",
            "task_number",
            "created",
            "state_changed_last_time",
            "receipt",
            "receipt_actual",
            "delivery_from",
            "delivery_to",
            "delivery_actual",
            "comment",
            "warehouse_id",
            "client_id",
            "client_address_id",
            "payment_type_id",
            "cost",
            "delivery_cost",
        }.AsReadOnly();

        public static readonly IReadOnlyList<string> TaskProductColumns = new List<string>
        {
            "product_id", 
            "quantity", 
        }.AsReadOnly();
        
        public static string GetTaskStateTransitionColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(TaskStateTransitionColumns, prefix);
        
        
        public static string GetTasksColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(TaskColumns, prefix);
        
        
        public static string GetTaskProductsColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(TaskProductColumns, prefix);

    }
}