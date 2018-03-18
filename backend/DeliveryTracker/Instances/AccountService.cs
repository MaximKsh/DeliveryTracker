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
        public async Task<ServiceResult<AccountServiceResult>> RegisterAsync(
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
                return new ServiceResult<AccountServiceResult>(validationResult.Error);
            }

            if (userInfo.Role == DefaultRoles.CreatorRole)
            {
                return new ServiceResult<AccountServiceResult>(ErrorFactory.AccessDenied());
            }
            if (userInfo.Role != DefaultRoles.ManagerRole
                && userInfo.Role != DefaultRoles.PerformerRole)
            {
                return new ServiceResult<AccountServiceResult>(ErrorFactory.RoleNotFound());
            }

            return await this.RegisterInternalAsync(codePassword, userInfo, oc);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<AccountServiceResult>> LoginAsync(
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
                return new ServiceResult<AccountServiceResult>(validationResult.Error);
            }
            
            var username = codePassword.Code;
            var password = codePassword.Password;

            var validateResult = await this.securityManager.ValidatePasswordAsync(username, password, oc);
            if (!validateResult.Success)
            {
                return new ServiceResult<AccountServiceResult>(validateResult.Errors);
            }

            var credentials = validateResult.Result;

            var userResult = await this.userManager.GetAsync(credentials.Id, credentials.InstanceId, oc: oc);
            if (!userResult.Success)
            {
                return new ServiceResult<AccountServiceResult>(userResult.Errors);
            }

            var result = new AccountServiceResult
            {
                User = userResult.Result,
                Credentials = credentials,
            };
            
            return new ServiceResult<AccountServiceResult>(result);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<AccountServiceResult>> LoginWithRegistrationAsync(
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
                        return new ServiceResult<AccountServiceResult>(
                            ErrorFactory.InvitaitonExpired(invitation.InvitationCode, invitation.Expires));
                    }
                    var deleteResult = await this.invitationService.DeleteAsync(invitation.InvitationCode);
                    if (!deleteResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<AccountServiceResult>(
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
                        return new ServiceResult<AccountServiceResult>(creationResult.Errors);
                    }
                    transaction.Commit();

                    var createdUser = creationResult.Result;
                    var userCredentials = new UserCredentials(
                        createdUser.Id,
                        createdUser.Code,
                        createdUser.Role,
                        createdUser.InstanceId);
                    var result = new AccountServiceResult
                    {
                        User = createdUser,
                        Credentials = userCredentials,
                        Registered = true,
                    };
                    
                    return new ServiceResult<AccountServiceResult>(result);
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<AccountServiceResult>> GetAsync(NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.userCredentialsAccessor.GetUserCredentials();
            var getResult = await this.userManager.GetAsync(credentials.Id, credentials.InstanceId, oc: oc);
            if (getResult.Success)
            {
                return new ServiceResult<AccountServiceResult>(new AccountServiceResult
                {
                    User = getResult.Result,
                });
            }
            return new ServiceResult<AccountServiceResult>(getResult.Errors);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<AccountServiceResult>> EditAsync(
            User newData, 
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.userCredentialsAccessor.GetUserCredentials();
            newData.Id = credentials.Id;
            newData.InstanceId = credentials.InstanceId;
            var editResult = await this.userManager.EditAsync(newData, oc);
            if (editResult.Success)
            {
                return new ServiceResult<AccountServiceResult>(new AccountServiceResult
                {
                    User = editResult.Result,
                });
            }
            return new ServiceResult<AccountServiceResult>(editResult.Errors);
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ChangePasswordAsync(
            string oldPassword, 
            string newPassword, 
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.userCredentialsAccessor.GetUserCredentials();
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

        private async Task<ServiceResult<AccountServiceResult>> RegisterInternalAsync(
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
                        return new ServiceResult<AccountServiceResult>(createUserResult.Errors);
                    }

                    var newUser = createUserResult.Result;

                    var setPasswordResult = 
                        await this.securityManager.SetPasswordAsync(newUser.Id, codePassword.Password, conn);
                    if (!setPasswordResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<AccountServiceResult>(setPasswordResult.Errors);
                    }

                    var credentials = setPasswordResult.Result;
                    transaction.Commit();

                    var result = new AccountServiceResult
                    {
                        User = newUser,
                        Credentials = credentials,
                    };
                    
                    return new ServiceResult<AccountServiceResult>(result);
                }
            }
        }
        
        #endregion
    }
}