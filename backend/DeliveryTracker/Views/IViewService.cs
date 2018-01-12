﻿using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Instances;
using DeliveryTracker.Services;

namespace DeliveryTracker.Views
{
    public interface IViewService<T>
    {
        string GroupName { get; }

        ServiceResult<IList<T>> GetViewResult(
            UserCredentials userCredentials,
            string viewName,
            IReadOnlyDictionary<string, string[]> parameters);

    }
}