﻿using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Views
{
    public static class ViewExtensions
    {
        #region IServiceCollection
        
        public static IServiceCollection AddDeliveryTrackerViews(this IServiceCollection services)
        {
            services
                .AddSingleton<IViewService, ViewService>()
                .AddSingleton<IViewGroup, ReferenceViewGroup>()
                .AddSingleton<IViewGroup, UserViewGroup>()
                .AddSingleton<IViewGroup, TaskViewGroup>()
                .AddSingleton<IViewGroup, AuxTaskViewGroup>()
                ;
            
            return services;
        }
        
        #endregion
        
        #region parameters

        public static bool TryGetValuesList(
            this IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            string key,
            out IReadOnlyList<string> list)
        {
            return parameters.TryGetValue(key, out list);
        }

        public static bool TryGetOneValue(
            this IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            string key,
            out string value)
        {
            value = null;
            if (parameters.TryGetValue(key, out var list)
                && list.Count > 0)
            {
                value = list[0];
                return true;
            }

            return false;
        }
        
        #endregion
    }
}