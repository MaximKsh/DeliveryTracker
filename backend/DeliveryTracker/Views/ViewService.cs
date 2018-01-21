using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DeliveryTracker.Common;
using DeliveryTracker.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Views
{
    public class ViewService : IViewService
    {
        #region fields
        
        private readonly ImmutableDictionary<string, IViewGroup> viewGroups;
        
        #endregion
        
        #region constructor
        
        public ViewService(IServiceProvider serviceProvider)
        {
            var groups = new Dictionary<string, IViewGroup>();
            foreach (var viewGroup in serviceProvider.GetServices<IViewGroup>())
            {
                groups[viewGroup.Name] = viewGroup;
            }

            this.viewGroups = groups.ToImmutableDictionary();
        }
        
        #endregion
        
        #region public

        /// <inheritdoc />
        public ServiceResult<string[]> GetViewGroupsList()
        {
            return new ServiceResult<string[]>(this.viewGroups.Keys.ToArray());
        }

        /// <inheritdoc />
        public ServiceResult<IViewGroup> GetViewGroup(string groupName)
        {
            if (this.viewGroups.TryGetValue(groupName, out var group))
            {
                return new ServiceResult<IViewGroup>(group);
            }

            return new ServiceResult<IViewGroup>(ErrorFactory.ViewGroupNotFound(groupName));
        }
        
        #endregion
    }
}