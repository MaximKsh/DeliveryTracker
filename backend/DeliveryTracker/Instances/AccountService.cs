using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.DbModels;
using DeliveryTracker.Identification;
using DeliveryTracker.Services;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//using DeliveryTracker.Auth;

namespace DeliveryTracker.Instances
{
    public class AccountService : IAccountService
    {
        #region constants

        private const int InvitationExpirationPeriodInDays = 3;

        private const int InvitationCodeLength = 10;

        private static readonly char[] InvitationCodeAlphabet =
            "23456789qwertyupasdfghkzxbnmQWERTYUPASDFGHKZXVBNM".ToCharArray();

        private static readonly Random Random = new Random();

        #endregion

        #region field
        
        private readonly IPostgresConnectionProvider cp;

        private readonly ILogger<AccountService> logger;

        #endregion

        #region constuctor

        public AccountService(
            IPostgresConnectionProvider cp, 
            ILogger<AccountService> logger)
        {
            this.cp = cp;
            this.logger = logger;
        }

        #endregion

        #region public

        public async Task<ServiceResult<string>> GetUserRole(UserModel user)
        {
           throw new NotImplementedException();
        }
    
        private async Task<ServiceResult<UserModel>> FindUserAsync(string username)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Попытаться получить пришлашение по коду.
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <param name="invitation"></param>
        /// <returns></returns>
        private bool TryGetInvitaiton(string invitationCode, out InvitationModel invitation)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> RegisterAsync(
            User userInfo,
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null)
        {
            // Делаем копию, т.к. меняем и возвращаем новый.
            var userInfoCopied = new User();
            userInfoCopied.Deserialize(userInfo.Serialize());

            userInfoCopied.Id = Guid.NewGuid();
            
            using (var conn = oc ?? this.cp.Create())
            {
                
            }
            
            /*
            var newUser = new UserModel
            {
                Id = Guid.NewGuid(),
                InstanceId = userInfo.Instance.Id,
                UserName = codePassword.Username,
                Surname = userInfo.Surname,
                Name = userInfo.Name,
                PhoneNumber = userInfo.PhoneNumber,
            };

            var result = await this.userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                // TODO: разбор ошибок.
                return new ServiceResult<Tuple<User, UserCredentials>>(
                    null,
                    result.Errors.Select(ErrorFactory.IdentityError));
            }
            result = await this.userManager.AddPasswordAsync(newUser, codePassword.Password);
            if (!result.Succeeded)
            {
                // TODO: разбор ошибок.
                return new ServiceResult<Tuple<User, UserCredentials>>(
                    null,
                    result.Errors.Select(ErrorFactory.IdentityError));
            }
            result = await this.userManager.AddToRoleAsync(newUser, userInfo.Role);
            if (!result.Succeeded)
            {
                // TODO: разбор ошибок.
                return new ServiceResult<Tuple<User, UserCredentials>>(
                    null,
                    result.Errors.Select(ErrorFactory.IdentityError));
            }
*/
            throw new NotImplementedException();
           // var registeredUser = new User(newUser);
           // var userCredentials = new UserCredentials(newUser.UserName, user.Role, newUser.InstanceId);

            //return new ServiceResult<Tuple<User, UserCredentials>>(
            //    new Tuple<User, UserCredentials>(registeredUser, userCredentials));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> LoginAsync(CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null)
        {
            var username = codePassword.Username;
            var password = codePassword.Password;
/*
            var userResult = await this.FindUserAsync(username);
            if (!userResult.Success)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(userResult.Errors);
            }
            if (!await this.userManager.CheckPasswordAsync(userResult.Result, password))
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(ErrorFactory.AccessDenied());
            }

            var user = userResult.Result;
            var roleResult = await this.GetUserRole(user);
            if (!roleResult.Success)
            {
                return new ServiceResult<Tuple<User, UserCredentials>>(ErrorFactory.UserWithoutRole(user.UserName));
            }*/
            throw new NotImplementedException();
            //var role = roleResult.Result;
            //var registeredUser = new User(user);
            //var userCredentials = new UserCredentials(user.UserName, role, user.InstanceId);
            //return new ServiceResult<Tuple<User, UserCredentials>>(
            //    new Tuple<User, UserCredentials>(registeredUser, userCredentials));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> LoginWithRegistrationAsync(
            CodePassword codePassword, NpgsqlConnectionWrapper oc = null)
        {
            /*using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var loginResult = await this.LoginAsync(codePassword);
                    if (loginResult.Success)
                    {
                        transaction.Commit();
                        return loginResult;
                    }
                    
                    InvitationModel invitationModel = null;
                    foreach (var error in loginResult.Errors)
                    {
                        if (error.Code == ErrorCode.UserNotFound
                            && this.TryGetInvitaiton(codePassword.Username, out invitationModel))
                        {
                            break;
                        }
                    }

                    if (invitationModel == null)
                    {
                        // Несмотря на ошибку, никаких изменений не произведено.
                        transaction.Commit();
                        return loginResult;
                    }

                    if (loginResult.Errors.All(p => p.Code != ErrorCode.UserNotFound)
                        || !this.TryGetInvitaiton(codePassword.Username, out var invitation))
                    {
                        // Несмотря на ошибку, никаких изменений не произведено.
                        transaction.Commit();
                        return new ServiceResult<Tuple<User, UserCredentials>>(
                            ErrorFactory.UserNotFound(codePassword.Username));
                    }

                    var newUser = new User
                    {
                        //InvitationCode = usernamePassword.Username,
                        Surname = invitation.Surname,
                        Name = invitation.Name,
                        PhoneNumber = invitation.PhoneNumber,
                        InstanceIf = new Instance(invitation.Instance),
                        Role = invitation.Role.Name,
                    };

                    this.dbContext.Invitations.Remove(invitation);
                    var registrationResult = await this.RegisterAsync(newUser, codePassword);
                    if (registrationResult.Success)
                    {
                        await this.dbContext.SaveChangesAsync();
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }

                    return registrationResult;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    this.logger.LogError(e, "LoginWithRegistrationAsync");
                }
            }*/
            return new ServiceResult<Tuple<User, UserCredentials>>(ErrorFactory.ServerError());
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult> EditAsync(Guid userId, User newData, NpgsqlConnectionWrapper oc = null)
        {
            /*
            var userResult = await this.FindUserAsync(username);
            if (!userResult.Success)
            {
                return new ServiceResult(userResult.Errors);
            }

            var user = userResult.Result;
            if (!string.IsNullOrWhiteSpace(newData.Surname))
            {
                user.Surname = newData.Surname;
            }
            if (!string.IsNullOrWhiteSpace(newData.Name))
            {
                user.Name = newData.Name;
            }
            if (!string.IsNullOrWhiteSpace(newData.PhoneNumber))
            {
                user.PhoneNumber = newData.PhoneNumber;
            }*/

            return new ServiceResult();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ValidatePasswordAsync(CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null)
        {
            var userResult = await this.FindUserAsync(codePassword.Username);
            
            return new ServiceResult();
        }

        public Task<ServiceResult> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword, NpgsqlConnectionWrapper oc = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ChangePasswordAsync(string username,
            string oldPassword,
            string newPassword, NpgsqlConnectionWrapper oc = null)
        {
            var userResult = await this.FindUserAsync(username);
            if (!userResult.Success)
            {
                return new ServiceResult(userResult.Errors);
            }

            var user = userResult.Result;

            throw new NotImplementedException();
        }
    }
}