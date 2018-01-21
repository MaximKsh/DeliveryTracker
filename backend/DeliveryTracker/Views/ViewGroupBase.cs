using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        
        protected ImmutableDictionary<string, IView> Views;
        
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
        public virtual ServiceResult<string[]> GetViewsList()
        {
            return new ServiceResult<string[]>(this.Views.Keys.ToArray());
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<Dictionary<string, long>>> GetDigestAsync(
            NpgsqlConnectionWrapper oc = null)
        {
            var parameters = new Dictionary<string, string[]>().ToImmutableDictionary();
            var credentials = this.Accessor.UserCredentials;
            var digest = new Dictionary<string, long>(this.Views.Count);
            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                foreach (var view in this.Views)
                {
                    var result = await view.Value.GetCountAsync(
                        conn,
                        credentials,
                        parameters);
                    if (!result.Success)
                    {
                        return new ServiceResult<Dictionary<string, long>>(result.Errors);
                    }
                    
                    digest[view.Key] = result.Result;
                }    
            }
            
            return new ServiceResult<Dictionary<string, long>>(digest);
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<T[]>> ExecuteViewAsync<T>(string viewName,
            IImmutableDictionary<string, string[]> parameters,
            NpgsqlConnectionWrapper oc = null)
        {
            var result = await this.ExecuteViewAsync(viewName, parameters, oc);
            if (!result.Success)
            {
                return new ServiceResult<T[]>(result.Errors);
            }

            var viewResult = result.Result;
            var typedArray = new T[viewResult.Length];
            try
            {
                Array.Copy(viewResult, typedArray, viewResult.Length);
                return new ServiceResult<T[]>(typedArray);
            }
            catch (Exception)
            {
                return new ServiceResult<T[]> (ErrorFactory.ViewResultTypeError(this.Name, viewName));
            }
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<object[]>> ExecuteViewAsync(string viewName,
            IImmutableDictionary<string, string[]> parameters,
            NpgsqlConnectionWrapper oc = null)
        {
            if (!this.Views.TryGetValue(viewName, out var view))
            {
                return new ServiceResult<object[]>(ErrorFactory.ViewNotFound(this.Name, viewName));
            }
            
            var credentials = this.Accessor.UserCredentials;
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
    }
}