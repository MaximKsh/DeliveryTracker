﻿using System;
using System.Data;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.References;
using DeliveryTracker.Tasks.Routing;
using DeliveryTracker.Tasks.TaskObservers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Tasks
{
    public static class TaskExtensions
    {
        public static IServiceCollection AddDeliveryTrackerTasks(this IServiceCollection services)
        {
            services
                .AddSingleton<ITaskStateTransitionManager, TaskStateTransitionManager>()
                .AddSingleton<ITaskManager, TaskManager>()
                .AddSingleton<ITaskService, TaskService>()
                .AddSingleton<ITaskObserverExecutor, TaskObserverExecutor>()
                .AddSingleton<IRoutingService, RoutingService>()
                ;

            // Observers
            services
                .AddSingleton<ITaskObserver, NotificationObserver>()
                .AddSingleton<ITaskObserver, TaskStatisticsObserver>()
                .AddSingleton<ITaskObserver, RouteRebuildObserver>()
                ;
            return services;
        }
        
        public static ISettingsStorage AddDeliveryTrackerTaskSettings(
            this ISettingsStorage storage, 
            IConfiguration configuration)
        {
            var routingSettings = TaskHelper.ReadRoutingSettingsFromConf(configuration);
            return storage
                    .RegisterSettings(routingSettings)
                ;
        }

        public static TaskState GetState(
            this TaskInfo tInfo)
        {
            return DefaultTaskStates.AllTaskStates.TryGetValue(tInfo.TaskStateId, out var state)
                ? state
                : new TaskState(tInfo.TaskStateId, tInfo.TaskStateName, tInfo.TaskStateCaption);
        }

        public static void SetState(
            this TaskInfo tInfo,
            TaskState state)
        {
            tInfo.TaskStateId = state.Id;
            tInfo.TaskStateName = state.Name;
            tInfo.TaskStateCaption = state.Caption;
        }
        
        #region IDataReader
        
        public static TaskStateTransition GetTaskStateTransition(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetTaskStateTransition(ref idx);
        }
        
        public static TaskStateTransition GetTaskStateTransition(this IDataReader reader, ref int idx)
        {
            return new TaskStateTransition
            {
                Id = reader.GetGuid(idx++),
                Role = reader.GetGuid(idx++),
                InitialState = reader.GetGuid(idx++),
                FinalState = reader.GetGuid(idx++),
                ButtonCaption = reader.GetString(idx++),
            };
        }
        
        public static TaskInfo GetTaskInfo(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetTaskInfo(ref idx);
        }
        
        public static TaskInfo GetTaskInfo(this IDataReader reader, ref int idx)
        {
            var taskInfo = new TaskInfo
            {
                Id = reader.GetGuid(idx++),
                InstanceId = reader.GetGuid(idx++),
                TaskStateId = reader.GetGuid(idx++),
                AuthorId = reader.GetGuid(idx++),
                PerformerId = reader.GetValueOrDefault<Guid?>(idx++),
                TaskNumber = reader.GetValueOrDefault<string>(idx++),
                Created = reader.GetValueOrDefault<DateTime?>(idx++),
                StateChangedLastTime = reader.GetValueOrDefault<DateTime?>(idx++),
                Receipt = reader.GetValueOrDefault<DateTime?>(idx++),
                ReceiptActual = reader.GetValueOrDefault<DateTime?>(idx++),
                DeliveryFrom = reader.GetValueOrDefault<DateTime?>(idx++),
                DeliveryTo = reader.GetValueOrDefault<DateTime?>(idx++),
                DeliveryEta = reader.GetValueOrDefault<DateTime?>(idx++),
                DeliveryActual = reader.GetValueOrDefault<DateTime>(idx++),
                Comment = reader.GetValueOrDefault<string>(idx++),
                WarehouseId = reader.GetValueOrDefault<Guid?>(idx++),
                ClientId = reader.GetValueOrDefault<Guid?>(idx++),
                ClientAddressId = reader.GetValueOrDefault<Guid?>(idx++),
                PaymentTypeId = reader.GetValueOrDefault<Guid?>(idx++),
                Cost = reader.GetValueOrDefault<decimal?>(idx++),
                DeliveryCost = reader.GetValueOrDefault<decimal?>(idx++),
            };
            
            if (DefaultTaskStates.AllTaskStates.TryGetValue(taskInfo.TaskStateId, out var state))
            {
                taskInfo.SetState(state);
            }

            return taskInfo;
        }
        
        public static TaskProduct GetTaskProduct(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetTaskProduct(ref idx);
        }
        
        public static TaskProduct GetTaskProduct(this IDataReader reader, ref int idx)
        {
            var taskProduct = new TaskProduct();
            taskProduct.Id = reader.GetGuid(idx++);
            taskProduct.InstanceId = reader.GetGuid(idx++);
            reader.SetCollectionReferenceBaseFields(taskProduct, ref idx);
            taskProduct.ProductId = reader.GetGuid(idx++);
            taskProduct.Quantity = reader.GetInt32(idx++);
            return taskProduct;
        }
        
        #endregion
    }
}