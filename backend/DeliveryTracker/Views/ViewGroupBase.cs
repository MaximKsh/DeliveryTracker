﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Views
{
    public abstract class ViewGroupBase : IViewGroup
    {
        #region fields

        protected readonly IPostgresConnectionProvider Cp;

        protected readonly IUserCredentialsAccessor Accessor;
        
        protected IReadOnlyDictionary<string, IView> Views;
        
        #endregion
        
        #region constructor
        
        protected ViewGroupBase(IPostgresConnectionProvider cp, IUserCredentialsAccessor accessor)
        {
            this.Cp = cp;
            this.Accessor = accessor;
        }
        
        #endregion
        
        #region public
        
        /// <inheritdoc />
        public abstract string Name { get; }
        
        /// <inheritdoc />
        public virtual ServiceResult<IList<string>> GetViewsList()
        {
            var credentials = this.Accessor.GetUserCredentials();
            var list = new List<string>(this.Views.Count);
            foreach (var view in this.Views)
            {
                if (view.Value.PermittedRoles.Contains(credentials.Role))
                {
                    list.Add(view.Key);
                }
            }
            
            return new ServiceResult<IList<string>>(list);
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<Dictionary<string, ViewDigest>>> GetDigestAsync(
            NpgsqlConnectionWrapper oc = null)
        {
            var parameters = new Dictionary<string, IReadOnlyList<string>>();
            var credentials = this.Accessor.GetUserCredentials();
            var digest = new Dictionary<string, ViewDigest>(this.Views.Count);
            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                foreach (var view in this.Views)
                {
                    if (!view.Value.PermittedRoles.Contains(credentials.Role))
                    {
                        continue;
                    }
                    var result = await view.Value.GetViewDigestAsync(conn, credentials, parameters);
                    if (!result.Success)
                    {
                        return new ServiceResult<Dictionary<string, ViewDigest>>(result.Errors);
                    }

                    digest[view.Key] = result.Result;
                }    
            }
            
            return new ServiceResult<Dictionary<string, ViewDigest>>(digest);
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<IList<T>>> ExecuteViewAsync<T>(
            string viewName,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlConnectionWrapper oc = null)
        {
            var result = await this.ExecuteViewAsync(viewName, parameters, oc);
            if (!result.Success)
            {
                return new ServiceResult<IList<T>>(result.Errors);
            }

            var viewResult = result.Result;
            try
            {
                return new ServiceResult<IList<T>>(viewResult.Cast<T>().ToList());
            }
            catch (Exception)
            {
                return new ServiceResult<IList<T>> (ErrorFactory.ViewResultTypeError(this.Name, viewName));
            }
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<IList<IDictionaryObject>>> ExecuteViewAsync(
            string viewName,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlConnectionWrapper oc = null)
        {
            if (!this.Views.TryGetValue(viewName, out var view))
            {
                return new ServiceResult<IList<IDictionaryObject>>(ErrorFactory.ViewNotFound(this.Name, viewName));
            }
            
            var credentials = this.Accessor.GetUserCredentials();
            
            if (!view.PermittedRoles.Contains(credentials.Role))
            {
                return new ServiceResult<IList<IDictionaryObject>>(ErrorFactory.AccessDenied());
            }
            
            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                return await view.GetViewResultAsync(
                    conn,
                    credentials,
                    parameters);
            }
        }
        
        #endregion
        
        #region protected

        protected void AddView(
            IView view,
            Dictionary<string, IView> views)
        {
            views[view.Name] = view;
        }
        
        #endregion
    }
}