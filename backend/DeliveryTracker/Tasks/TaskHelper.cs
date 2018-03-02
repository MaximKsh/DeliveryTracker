using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DeliveryTracker.Database;
using DeliveryTracker.Instances;

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

        public static string GetTaskStateTransitionColuns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(TaskStateTransitionColumns, prefix);

    }
}