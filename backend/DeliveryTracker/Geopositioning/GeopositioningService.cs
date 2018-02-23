using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.Geopositioning
{
    public class GeopositioningService : IGeopositioningService
    {
        #region sql

        private const string SqlSetNullGeoposition = @"
update ""users""
set ""geoposition"" = null
where ""id"" = @user_id
;
";
        
        private const string SqlSetGeoposition = @"
update ""users""
set ""geoposition"" = st_setsrid(ST_MakePoint(@lon, @lat), 4326)::geography
where ""id"" = @user_id
;
";
        
        #endregion
        
        #region fields
        
        private readonly IPostgresConnectionProvider cp;

        private readonly IUserCredentialsAccessor userCredentialsAccessor;
        
        #endregion
        
        #region constuctor

        public GeopositioningService(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor userCredentialsAccessor)
        {
            this.cp = cp;
            this.userCredentialsAccessor = userCredentialsAccessor;
        }
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public async Task<ServiceResult> UpdateGeopositionAsync(
            Geoposition geoposition,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.userCredentialsAccessor.GetUserCredentials();
            if (!credentials.Valid
                || credentials.Role != DefaultRoles.PerformerRole)
            {
                return new ServiceResult(ErrorFactory.AccessDenied());
            }

            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.Parameters.Add(new NpgsqlParameter("user_id", credentials.Id));
                    if (geoposition != null)
                    {
                        command.CommandText = SqlSetGeoposition;
                        command.Parameters.Add(new NpgsqlParameter("lon", geoposition.Longitude));
                        command.Parameters.Add(new NpgsqlParameter("lat", geoposition.Latitude));
                    }
                    else
                    {
                        command.CommandText = SqlSetNullGeoposition;
                    }

                    var affectedRows = await command.ExecuteNonQueryAsync();

                    return affectedRows != 0
                        ? new ServiceResult()
                        : new ServiceResult(ErrorFactory.UserNotFound(credentials.Id));
                }
            }
            
        }
        
        #endregion
    }
}