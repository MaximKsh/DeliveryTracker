using System;
using DeliveryTracker.Common;
using DeliveryTracker.Services;

namespace DeliveryTracker.Instances
{
    public interface IUserService
    {
        ServiceResult<Invitation> CreateInvitation(User preliminaryUser);

        ServiceResult<Invitation> GetInvitation(Guid invitationId);
        
        ServiceResult<Invitation> GetInvitation(string invitationCode);
        
        ServiceResult DeleteInvitation(Guid invitationId);
        
        ServiceResult DeleteInvitation(string invitationCode);
        
        ServiceResult<User> Get(string userName);
        
        ServiceResult Edit(User newData);

        ServiceResult Delete(string userName);
    }
}