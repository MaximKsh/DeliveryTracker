using Microsoft.AspNetCore.Http;

namespace DeliveryTracker.Identification
{
    public class UserCredentialsAccessor : IUserCredentialsAccessor
    {
        #region fields
        
        private readonly IHttpContextAccessor ctxAccessor;
        
        #endregion
        
        #region constructor
        
        public UserCredentialsAccessor(IHttpContextAccessor ctxAccessor)
        {
            this.ctxAccessor = ctxAccessor;
        }
        
        #endregion

        #region public
        
        /// <inheritdoc />
        public UserCredentials GetUserCredentials() => this.CreateFromContext();

        #endregion
        
        #region private
        
        private UserCredentials CreateFromContext()
        {
            var ctx = this.ctxAccessor.HttpContext;
            return ctx.User.Claims.ToUserCredentials();
        }
        
        #endregion
    }
}