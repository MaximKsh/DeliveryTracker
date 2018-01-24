using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Microsoft.Extensions.Logging;


namespace DeliveryTracker.Instances
{
    public class AccountService : IAccountService
    {
        #region field
        
        private readonly IPostgresConnectionProvider cp;

        private readonly IUserCredentialsAccessor userCredentialsAccessor;
        
        private readonly IUserManager userManager;

        private readonly ISecurityManager securityManager;

        private readonly IInvitationService invitationService;

        private readonly ILogger<IAccountService> logger;

        #endregion

        #region constuctor

        public AccountService(
            IPostgresConnectionProvider cp, 
            IUserCredentialsAccessor userCredentialsAccessor,
            IUserManager userManager,
            ISecurityManager securityManager,
            IInvitationService invitationService,
            ILogger<IAccountService> logger)
        {
            this.cp = cp;
            this.userCredentialsAccessor = userCredentialsAccessor;
            this.userManager = userManager;
            this.securityManager = securityManager;
            this.invitationService = invitationService;
            this.logger = logger;
        }

        #endregion

        #region public

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> RegisterAsync(
            CodePassword codePassword,
            Action<User> userModificationAction = null,
            NpgsqlConnectionWrapper oc = null)
        {
            var userInfo = new User { Id = Guid.NewGuid(), Code = codePassword.Code};
            userModificationAction?.Invoke(userInfo);
            
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule("CodePassword.Code", codePassword.Code)
                .AddNotNullOrWhitespaceRule("CodePassword.Password", codePassword.Password)
                .AddNotEmptyGuidRule("User.InstanceId", userInfo.InstanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(validationResult.Error);
            }

            if (userInfo.Role == DefaultRoles.CreatorRole)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(ErrorFactory.AccessDenied());
            }
            if (userInfo.Role != DefaultRoles.ManagerRole
                && userInfo.Role != DefaultRoles.PerformerRole)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(ErrorFactory.RoleNotFound());
            }

            return await this.RegisterInternalAsync(codePassword, userInfo, oc);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> LoginAsync(
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule("CodePassword", codePassword)
                .AddNotNullOrWhitespaceRule("CodePassword.Code", codePassword.Code)
                .AddNotNullOrWhitespaceRule("CodePassword.Password", codePassword.Password)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(validationResult.Error);
            }
            
            var username = codePassword.Code;
            var password = codePassword.Password;

            var validateResult = await this.securityManager.ValidatePasswordAsync(username, password, oc);
            if (!validateResult.Success)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(validateResult.Errors);
            }

            var credentials = validateResult.Result;

            var userResult = await this.userManager.GetAsync(credentials.Id, credentials.InstanceId, oc);
            if (!userResult.Success)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(userResult.Errors);
            }
            return new ServiceResult<Tuple<User, UserCredentials>>(
                new Tuple<User, UserCredentials>(userResult.Result, credentials));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> LoginWithRegistrationAsync(
            CodePassword codePassword, 
            NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var transaction = conn.BeginTransaction())
                {
                    var loginResult = await this.LoginAsync(codePassword, conn);
                    if (loginResult.Success)
                    {
                        transaction.Commit();
                        return loginResult;
                    }
                    
                    Invitation invitation = null;
                    foreach (var error in loginResult.Errors)
                    {
                        if (error.Code != ErrorCode.AccessDenied)
                        {
                            continue;
                        }
                        var getResult = await this.invitationService.GetAsync(codePassword.Code, conn);
                        if (!getResult.Success)
                        {
                            continue;
                        }
                        invitation = getResult.Result;
                        break;
                    }
                    if (invitation == null)
                    {
                        transaction.Rollback();
                        return loginResult;
                    }

                    if (invitation.Expires < DateTime.UtcNow)
                    {
                        transaction.Rollback();
                        return new ServiceResult<Tuple<User, UserCredentials>>(
                            ErrorFactory.InvitaitonExpired(invitation.InvitationCode, invitation.Expires));
                    }
                    var deleteResult = await this.invitationService.DeleteAsync(invitation.InvitationCode);
                    if (!deleteResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<Tuple<User, UserCredentials>>(
                            ErrorFactory.InvitationNotFound(invitation.InvitationCode));
                    }

                    var newUser = new User()
                    {
                        Id = Guid.NewGuid(),
                        Code = invitation.InvitationCode,
                        Role = invitation.Role,
                        InstanceId = invitation.InstanceId,
                        Surname = invitation.PreliminaryUser.Surname,
                        Name = invitation.PreliminaryUser.Name,
                        Patronymic = invitation.PreliminaryUser.Patronymic,
                        PhoneNumber = invitation.PreliminaryUser.PhoneNumber,
                    };

                    
                    var creationResult = await this.userManager.CreateAsync(newUser, conn);
                    if (!creationResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<Tuple<User, UserCredentials>>(creationResult.Errors);
                    }
                    transaction.Commit();

                    var createdUser = creationResult.Result;
                    var userCredentials = new UserCredentials(
                        createdUser.Id,
                        createdUser.Code,
                        createdUser.Role,
                        createdUser.InstanceId);
                    return new ServiceResult<Tuple<User, UserCredentials>>(
                        new Tuple<User, UserCredentials> (createdUser, userCredentials));
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<User>> GetAsync(NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.userCredentialsAccessor.UserCredentials;
            return await this.userManager.GetAsync(credentials.Id, credentials.InstanceId, oc);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<User>> EditAsync(
            User newData, 
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.userCredentialsAccessor.UserCredentials;
            newData.Id = credentials.Id;
            newData.InstanceId = credentials.InstanceId;
            
            return await this.userManager.EditAsync(newData, oc);
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ChangePasswordAsync(
            string oldPassword, 
            string newPassword, 
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.userCredentialsAccessor.UserCredentials;
            var userId = credentials.Id;
            
            var verificationResult = await this.securityManager.ValidatePasswordAsync(userId, oldPassword, oc);
            if (!verificationResult.Success)
            {
                return new ServiceResult(verificationResult.Errors);
            }

            var changePasswordResult = await this.securityManager.SetPasswordAsync(userId, newPassword, oc);
            return changePasswordResult.Success 
                ?  new ServiceResult(changePasswordResult.Errors) 
                :  new ServiceResult();
        }

        
        #endregion
        
        #region private

        private async Task<ServiceResult<Tuple<User, UserCredentials>>> RegisterInternalAsync(
            CodePassword codePassword,
            User userInfo,
            NpgsqlConnectionWrapper oc)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var transaction = conn.BeginTransaction())
                {
                    var createUserResult = await this.userManager.CreateAsync(userInfo, conn);
                    if (!createUserResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<Tuple<User, UserCredentials>>(createUserResult.Errors);
                    }

                    var newUser = createUserResult.Result;

                    var setPasswordResult = 
                        await this.securityManager.SetPasswordAsync(newUser.Id, codePassword.Password, conn);
                    if (!setPasswordResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<Tuple<User, UserCredentials>>(setPasswordResult.Errors);
                    }

                    var credentials = setPasswordResult.Result;
                    transaction.Commit();
                    
                    return new ServiceResult<Tuple<User, UserCredentials>>(
                        new Tuple<User, UserCredentials>(newUser, credentials));
                }
            }
        }
        
        #endregion
    }
}