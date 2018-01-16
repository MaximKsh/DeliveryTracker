using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Microsoft.Extensions.Logging;

//using DeliveryTracker.Auth;

namespace DeliveryTracker.Instances
{
    public class AccountService : IAccountService
    {
        #region field
        
        private readonly IPostgresConnectionProvider cp;

        private readonly IUserManager userManager;

        private readonly ISecurityManager securityManager;

        private readonly IInvitationService invitationService;

        private readonly ILogger<AccountService> logger;

        #endregion

        #region constuctor

        public AccountService(
            IPostgresConnectionProvider cp, 
            IUserManager userManager,
            ISecurityManager securityManager,
            IInvitationService invitationService,
            ILogger<AccountService> logger)
        {
            this.cp = cp;
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
            var userInfo = new User { Id = Guid.NewGuid() };
            userModificationAction?.Invoke(userInfo);
            
            var validationResult = new ParametersValidator()
                .AddRule("CodePassword.Code", codePassword.Code, x => !string.IsNullOrEmpty(x))
                .AddRule("CodePassword.Password", codePassword.Password, x => !string.IsNullOrEmpty(x))
                .AddRule("User.Surname", userInfo.Surname, x => x != null && !string.IsNullOrEmpty(x))
                .AddRule("User.Name", userInfo.Surname, x => x != null && !string.IsNullOrEmpty(x))
                .AddRule("User.Role", userInfo.Role, x => x != null && !string.IsNullOrEmpty(x))
                .AddRule("User.InstanceId", userInfo.InstanceId, x => x != Guid.Empty)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(validationResult.Error);
            }
            
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

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> LoginAsync(
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null)
        {
            var username = codePassword.Code;
            var password = codePassword.Password;

            var validateResult = await this.securityManager.ValidatePasswordAsync(username, password, oc);
            if (!validateResult.Success)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(validateResult.Errors);
            }

            var credentials = validateResult.Result;

            var userResult = await this.userManager.GetAsync(credentials.Id, credentials.Id, oc);
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
                        if (error.Code != ErrorCode.UserNotFound)
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
                        // Несмотря на ошибку, никаких изменений не произведено.
                        transaction.Commit();
                        return loginResult;
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
                        return new ServiceResult<Tuple<User, UserCredentials>>(creationResult.Errors);
                    }

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
        public async Task<ServiceResult<User>> EditAsync(
            User newData, 
            NpgsqlConnectionWrapper oc = null)
        {
            return await this.userManager.EditAsync(newData, oc);
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ChangePasswordAsync(
            Guid userId, 
            string oldPassword, 
            string newPassword, 
            NpgsqlConnectionWrapper oc = null)
        {
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

        
        
        #endregion
    }
}