﻿using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Views
{
    public static class ViewExtensions
    {
        public static IServiceCollection AddDeliveryTrackerViews(this IServiceCollection services)
        {
            services
                .AddSingleton<IViewService, ViewService>()
                .AddSingleton<IViewGroup, ReferenceViewGroup>()
                .AddSingleton<IViewGroup, UserViewGroup>()
                .AddSingleton<IViewGroup, TaskViewGroup>()
                ;
            
            return services;
        }
    }
}