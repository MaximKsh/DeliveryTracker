using System.Data;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Instances
{
    public static class InstanceExtensions
    {
        #region IServiceCollection
        
        public static IServiceCollection AddDeliveryTrackerInstances(
            this IServiceCollection services)
        {
            services
                .AddSingleton<IInvitationService, InvitationService>()
                .AddSingleton<IAccountService, AccountService>()
                .AddSingleton<IInstanceService, InstanceService>()
                .AddSingleton<IUserService, UserService>();

            return services;
        }
        
        public static ISettingsStorage AddDeliveryTrackerInstancesSettings(
            this ISettingsStorage storage, 
            IConfiguration configuration)
        {
            
            var invitationSettings = InstanceHelper.ReadInvitationSettingsFromConf(configuration);

            return storage
                    .RegisterSettings(invitationSettings)
                ;
        }
        
        #endregion
        
        #region IDataReader
        
        public static Instance GetInstance(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetInstance(ref idx);
        }
        
        public static Instance GetInstance(this IDataReader reader, ref int idx)
        {
            return new Instance
            {
                Id = reader.GetGuid(idx++),
                Name = reader.GetString(idx++),
                CreatorId = reader.GetGuid(idx++)
            };
        }
        
        public static Invitation GetInvitation(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetInvitation(ref idx);
        }
        
        public static Invitation GetInvitation(this IDataReader reader, ref int idx)
        {
            return new Invitation
            {
                Id = reader.GetGuid(idx++),
                InvitationCode = reader.GetString(idx++),
                CreatorId = reader.GetGuid(idx++),
                Created = reader.GetDateTime(idx++),
                Expires = reader.GetDateTime(idx++),
                Role = reader.GetGuid(idx++),
                InstanceId = reader.GetGuid(idx++),
                PreliminaryUser = new User()
                {
                    Surname = reader.GetValueOrDefault<string>(idx++),
                    Name = reader.GetValueOrDefault<string>(idx++),
                    Patronymic = reader.GetValueOrDefault<string>(idx++),
                    PhoneNumber = reader.GetValueOrDefault<string>(idx++),
                }
            };
        }
        
        #endregion
    }
}