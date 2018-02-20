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
        public virtual async Task<ServiceResult<Dictionary<string, ViewDigest>>> GetDigestAsync(
            NpgsqlConnectionWrapper oc = null)
        {
            var parameters = new Dictionary<string, string[]>().ToImmutableDictionary();
            var credentials = this.Accessor.GetUserCredentials();
            var digest = new Dictionary<string, ViewDigest>(this.Views.Count);
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
                        return new ServiceResult<Dictionary<string, ViewDigest>>(result.Errors);
                    }

                    digest[view.Key] = new ViewDigest
                    {
                        Caption = view.Value.Caption,
                        Count = result.Result,
                    };
                }    
            }
            
            return new ServiceResult<Dictionary<string, ViewDigest>>(digest);
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<IList<T>>> ExecuteViewAsync<T>(string viewName,
            IImmutableDictionary<string, string[]> parameters,
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
        public virtual async Task<ServiceResult<IList<IDictionaryObject>>> ExecuteViewAsync(string viewName,
            IImmutableDictionary<string, string[]> parameters,
            NpgsqlConnectionWrapper oc = null)
        {
            if (!this.Views.TryGetValue(viewName, out var view))
            {
                return new ServiceResult<IList<IDictionaryObject>>(ErrorFactory.ViewNotFound(this.Name, viewName));
            }
            
            var credentials = this.Accessor.GetUserCredentials();
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