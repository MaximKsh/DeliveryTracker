using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.DbModels;
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

        #region fields

        private delegate Task<T> AsyncFunc<T>(UserModel user);
        
        private readonly UserManager<UserModel> userManager;
        
        private readonly DeliveryTrackerDbContext dbContext;
        
        private readonly RoleCache roleCache;

        private readonly ILogger<AccountService> logger;

        // private readonly AuthInfo authInfo;

        #endregion

        #region constuctor

        public AccountService(
            UserManager<UserModel> userManager,
            DeliveryTrackerDbContext dbContext,
            RoleCache roleCache/*, 
            AuthInfo authInfo*/)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
            this.roleCache = roleCache;
            //this.authInfo = authInfo;
        }

        #endregion

        #region public

        public async Task<ServiceResult<UserModel>> FindUser(string a)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<string>> GetUserRole(UserModel user)
        {
           throw new NotImplementedException();
        }
        public async Task<bool> IsInRole(UserModel user, params string[] expected)
        {
          throw new NotImplementedException();
        }
        
        
        /*
        

        /// <summary>
        /// Получить роль пользователя.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Роль пользователя или ошибка UserWithoutRole</returns>
        public async Task<ServiceResult<string>> GetUserRole(UserModel user)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }

            var role = (await this.userManager.GetRolesAsync(user))
                .FirstOrDefault(p => p == RoleAlias.Creator || p == RoleAlias.Manager || p == RoleAlias.Performer);
            
            return role != null 
                ? new ServiceResult<string>(role) 
                : new ServiceResult<string>(ErrorFactory.UserWithoutRole(user.UserName));
        }

        /// <summary>
        /// Входит ли пользователь в указанные роли.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public async Task<bool> IsInRole(UserModel user, params string[] expected)
        {
            var roleResult = await this.GetUserRole(user);
            if (!roleResult.Success)
            {
                return false;
            }
            var role = roleResult.Result;
            return expected.Any(p => p == role);
        }
        
        /// <summary>
        /// Входит ли пользователь в указанные роли.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public async Task<bool> IsInRole(UserModel user, params RoleModel[] expected)
        {
            var roleResult = await this.GetUserRole(user);
            if (!roleResult.Success)
            {
                return false;
            }
            var role = roleResult.Result;
            return expected.Select(p => p.Name).Any(p => p == role);
        }

        /// <summary>
        /// Изменить данные пользователя.
        /// Выполнять в транзакции.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="newData"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> Modify(string username, UserViewModel newData)
        {
            var userResult = await this.FindUser(username);
            if (!userResult.Success 
                || userResult.Result == null)
            {
                return new ServiceResult<UserModel>(userResult.Errors);
            }
            var user = userResult.Result;
            return await this.Modify(user, newData);
        }

        /// <summary>
        /// Изменить данные пользователя.
        /// Выполнять в транзакции.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newData"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> Modify(UserModel user, UserViewModel newData)
        {
            var modified = false;
            if (!string.IsNullOrWhiteSpace(newData.Surname))
            {
                modified = true;
                user.Surname = SecurityElement.Escape(newData.Surname);
            }
            
            if (!string.IsNullOrWhiteSpace(newData.Name))
            {
                modified = true;
                user.Name = SecurityElement.Escape(newData.Name);
            }
            
            if (!string.IsNullOrWhiteSpace(newData.PhoneNumber))
            {
                modified = true;
                user.PhoneNumber = SecurityElement.Escape(newData.PhoneNumber);
            }

            if (!modified)
            {
                return new ServiceResult<UserModel>(user);
            }
            var result = await this.userManager.UpdateAsync(user);
            return result.Succeeded 
                ? new ServiceResult<UserModel>(user) 
                : new ServiceResult<UserModel>(result.Errors.Select(ErrorFactory.IdentityError));
        }
        
        /// <summary>
        /// Смена пароля для пользователя.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="passwords"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> ChangePassword(
            string username,
            ChangePasswordViewModel passwords)
        {
            var userResult = await this.FindUser(username);
            if (!userResult.Success 
                || userResult.Result == null)
            {
                return new ServiceResult<UserModel>(userResult.Errors);
            }
            var user = userResult.Result;
            return await this.ChangePassword(user, passwords);
        }

        /// <summary>
        /// Смена пароля для пользователя.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwords"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> ChangePassword(
            UserModel user, 
            ChangePasswordViewModel passwords)
        {
            var result = await this.userManager.ChangePasswordAsync(
                user, 
                passwords.CurrentCredentials.Password,
                passwords.NewCredentials.Password);
            return result.Succeeded 
                ? new ServiceResult<UserModel>(user) 
                : new ServiceResult<UserModel>(result.Errors.Select(ErrorFactory.IdentityError));
        }

        /// <summary>
        /// Регистрация нового пользователя. Необходимо выполнять в транзакции.
        /// Username сгенерирован автоматически.
        /// </summary>
        /// <param name="credentialsInfo"></param>
        /// <param name="userInfo"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> Register(
            CredentialsViewModel credentialsInfo,
            UserViewModel userInfo,
            Guid instanceId)
        {
            var newUser = new UserModel
            {
                UserName = GenerateInvitationCode(),
                Surname = SecurityElement.Escape(userInfo.Surname),
                Name = SecurityElement.Escape(userInfo.Name),
                PhoneNumber = SecurityElement.Escape(userInfo.PhoneNumber),
                InstanceId = instanceId,
            };
            return await this.RegisterInternal(newUser, credentialsInfo.Password, userInfo.Role);
        }

        /// <summary>
        /// Регистрация нового пользователя по приглашению.
        /// Необходимо выполнять в транзакции.
        /// </summary>
        /// <param name="invitation"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> RegisterWithInvitation(
            InvitationModel invitation, 
            CredentialsViewModel credentials)
        {
            if (invitation.ExpirationDate < DateTime.UtcNow)
            {
                return new ServiceResult<UserModel>(ErrorFactory.InvitaitonExpired(invitation.InvitationCode));
            }
            
            this.dbContext.Invitations.Remove(invitation);
            
            var newUser = new UserModel
            {
                UserName = invitation.InvitationCode,
                Surname = invitation.Surname ?? string.Empty,
                Name = invitation.Name ?? string.Empty,
                PhoneNumber = invitation.PhoneNumber ?? string.Empty,
                InstanceId = invitation.InstanceId,
            };
            return await this.RegisterInternal(newUser, credentials.Password, invitation.Role.Name);
        }
        
        /// <summary>
        /// Выписывает токен для пользователя.
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TokenViewModel>> Login(CredentialsViewModel credentials)
        {
            var username = credentials.Username;
            var password = credentials.Password;
            
            var userResult = await this.FindUser(username);
            if (!userResult.Success 
                || userResult.Result == null)
            {
                return new ServiceResult<TokenViewModel>(userResult.Errors);
            }
            if (!await this.userManager.CheckPasswordAsync(userResult.Result, password))
            {
                return new ServiceResult<TokenViewModel>(ErrorFactory.AccessDenied());
            }

            var user = userResult.Result;
            var role = await this.GetUserRole(user);
            if (!role.Success)
            {
                return new ServiceResult<TokenViewModel>(ErrorFactory.UserWithoutRole(user.UserName));
            }

            var token = this.CreateToken(user.UserName, role.Result);

            return new ServiceResult<TokenViewModel>(new TokenViewModel
            {
                User = new UserViewModel
                {
                    Surname = user.Surname,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Role = role.Result,
                    Username = user.UserName,
                    Instance = new InstanceViewModel
                    {
                        InstanceName = user.Instance.InstanceName
                    }
                },
                Token = token,
            });
        }

        /// <summary>
        /// Создать приглашение для роли.
        /// </summary>
        /// <param name="currentUsername"></param>
        /// <param name="role"></param>
        /// <param name="preliminaryUserInfo"></param>
        /// <returns></returns>
        public async Task<ServiceResult<InvitationModel>> CreateInvitation(
            string currentUsername,
            RoleModel role,
            UserViewModel preliminaryUserInfo = null)
        {
            var userResult = await this.FindUser(currentUsername);
            if (!userResult.Success)
            {
                return new ServiceResult<InvitationModel>(null, userResult.Errors);
            }
            var user = userResult.Result;
            return await this.CreateInvitation(user, role, preliminaryUserInfo);
        }
        
        /// <summary>
        /// Создать приглашение для роли.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="role"></param>
        /// <param name="preliminaryUserInfo"></param>
        /// <returns></returns>
        public async Task<ServiceResult<InvitationModel>> CreateInvitation(
            UserModel currentUser,
            RoleModel role,
            UserViewModel preliminaryUserInfo = null)
        {
            if (!await this.UserCanCreateInvitaiton(currentUser, role))
            {
                return new ServiceResult<InvitationModel>(ErrorFactory.AccessDenied());
            }
            
            var invitation = new InvitationModel
            {
                Id = Guid.NewGuid(),
                InvitationCode = GenerateInvitationCode(),
                ExpirationDate = DateTime.UtcNow.AddDays(InvitationExpirationPeriodInDays),
                RoleId = role.Id,
                InstanceId = currentUser.InstanceId,
                Surname = SecurityElement.Escape(preliminaryUserInfo?.Surname) ?? string.Empty,
                Name = SecurityElement.Escape(preliminaryUserInfo?.Name) ?? string.Empty,
                PhoneNumber = SecurityElement.Escape(preliminaryUserInfo?.PhoneNumber) ?? string.Empty,
            };

            this.dbContext.Invitations.Add(invitation);

            return new ServiceResult<InvitationModel>(invitation);
        }

        
        
        /// <summary>
        /// Отметить пользователя как удаленного.
        /// Выполнять в транзакции.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> MarkUserAsDeleted(string username)
        {
            return await this.ProvideUserModelToFunc(
                username,
                this.MarkUserAsDeleted,
                errors => new ServiceResult<UserModel>(null, errors));
        }
        
        /// <summary>
        /// Отметить пользователя как удаленного.
        /// Выполнять в транзакции.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> MarkUserAsDeleted(UserModel user)
        {
            user.Deleted = true;
            var identityResult = await this.userManager.UpdateAsync(user);
            return identityResult.Succeeded 
                ? new ServiceResult<UserModel>(user) 
                : new ServiceResult<UserModel>(null, identityResult.Errors.Select(ErrorFactory.IdentityError));
        }

        /// <summary>
        /// Удалить пользователя окончательно.
        /// Выполнять в транзакции.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<ServiceResult> DeleteUser(string username)
        {
            return await this.ProvideUserModelToFunc(
                username,
                this.DeleteUser,
                errors => new ServiceResult(errors));
        }
        
        /// <summary>
        /// Удалить пользователя окончательно.
        /// Выполнять в транзакции.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ServiceResult> DeleteUser(UserModel user)
        {
            var identityResult = await this.userManager.DeleteAsync(user);
            return identityResult.Succeeded 
                ? new ServiceResult() 
                : new ServiceResult(identityResult.Errors.Select(ErrorFactory.IdentityError));
        }

        /// <summary>
        /// Обновить используемое пользователем устройство. 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<ServiceResult> UpdateUserDevice(string userName, DeviceViewModel device)
        {
            var userResult = await this.FindUser(userName);
            if (!userResult.Success)
            {
                return new ServiceResult(userResult.Errors);
            }
            return await this.UpdateUserDevice(userResult.Result, device);
        }
        
        /// <summary>
        /// Обновить используемое пользователем устройство. 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<ServiceResult> UpdateUserDevice(UserModel user, DeviceViewModel device)
        {
            var currentDevice = user.Device;
            if (currentDevice == null)
            {
                currentDevice = new DeviceModel
                {
                    Id = Guid.NewGuid(),
                    FirebaseId = device.FirebaseId,
                    UserId = user.Id,
                };
            }
            var result = await this.dbContext.Devices.AddAsync(currentDevice);
            return new ServiceResult();
        }
        
        #endregion

        #region private
        
        /// <summary>
        /// Генерация InvitationCode
        /// TODO: обратить внимание, нет проверки на то, что такой код уже был сгенерирован. Возможно повторение.
        /// </summary>
        /// <returns></returns>
        private static string GenerateInvitationCode() =>
                new string(Enumerable
                    .Range(0, InvitationCodeLength)
                    .Select(x => InvitationCodeAlphabet[Random.Next(0, InvitationCodeAlphabet.Length)])
                    .ToArray());

        private async Task<ServiceResult<UserModel>> RegisterInternal(
            UserModel newUser,
            string password,
            string roleName)
        {
            var result = await this.userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                return new ServiceResult<UserModel>(
                    null,
                    result.Errors.Select(ErrorFactory.IdentityError));
            }
            result = await this.userManager.AddPasswordAsync(newUser, password);
            if (!result.Succeeded)
            {
                return new ServiceResult<UserModel>(
                    null,
                    result.Errors.Select(ErrorFactory.IdentityError));
            }
            result = await this.userManager.AddToRoleAsync(newUser, roleName);
            if (!result.Succeeded)
            {
                return new ServiceResult<UserModel>(
                    null,
                    result.Errors.Select(ErrorFactory.IdentityError));
            }
            return new ServiceResult<UserModel>(newUser);
        }
        
        private async Task<T> ProvideUserModelToFunc<T>(
            string username,
            AsyncFunc<T> userModelFunc,
            Func<IEnumerable<IError>, T> nullServiceResultFunc)
        {
            var userResult = await this.FindUser(username);
            if (!userResult.Success)
            {
                return nullServiceResultFunc(userResult.Errors);
            }

            return await userModelFunc(userResult.Result);
        }
        
        private string CreateToken(string username, string role)
        {
            /*var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, username),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, role),
            };
            var identity = new ClaimsIdentity(
                claims,
                "Token",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                issuer: this.authInfo.Issuer,
                audience: this.authInfo.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.AddMinutes(this.authInfo.Lifetime),
                signingCredentials: new SigningCredentials(
                    this.authInfo.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
            throw new NotImplementedException();
        }
        
        private async Task<bool> UserCanCreateInvitaiton(UserModel user, RoleModel invitationRole)
        {
            var roleResult = await this.GetUserRole(user);
            if (!roleResult.Success)
            {
                return false;
            }
            var role = roleResult.Result;

            return role == this.roleCache.Creator.Name
                   || role == this.roleCache.Manager.Name && invitationRole == this.roleCache.Performer;
        }
    */
        /// <summary>
        /// Загрузить пользователя по имени.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>
        /// </returns>
        private async Task<ServiceResult<UserModel>> FindUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new ServiceResult<UserModel>(ErrorFactory.UserNotFound(username));
            }
            var currentUser = await this.userManager.FindByNameAsync(username);
            if (currentUser == null)
            {
                return new ServiceResult<UserModel>(ErrorFactory.UserNotFound(username));
            }
            if (currentUser.Deleted)
            {
                return new ServiceResult<UserModel>(ErrorFactory.UserDeleted(currentUser.Name));
            }
            //this.dbContext.Entry(currentUser).Reference(p => p.Device).Load();
            //this.dbContext.Entry(currentUser).Reference(p => p.Instance).Load();

            return new ServiceResult<UserModel>(currentUser);
        }

        /// <summary>
        /// Попытаться получить пришлашение по коду.
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <param name="invitation"></param>
        /// <returns></returns>
        private bool TryGetInvitaiton(string invitationCode, out InvitationModel invitation)
        {
            invitation = this.dbContext
                .Invitations
                .Include(p => p.Role)
                .Include(p => p.Instance)
                .FirstOrDefault(p => p.InvitationCode == invitationCode);
            return invitation != null;
        }

        #endregion

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> RegisterAsync(
            User user,
            UsernamePassword usernamePassword)
        {
            var newUser = new UserModel
            {
                Id = Guid.NewGuid(),
                InstanceId = user.Instance.Id,
                UserName = usernamePassword.Username,
                Surname = user.Surname,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
            };

            var result = await this.userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                // TODO: разбор ошибок.
                return new ServiceResult<Tuple<User, UserCredentials>>(
                    null,
                    result.Errors.Select(ErrorFactory.IdentityError));
            }
            result = await this.userManager.AddPasswordAsync(newUser, usernamePassword.Password);
            if (!result.Succeeded)
            {
                // TODO: разбор ошибок.
                return new ServiceResult<Tuple<User, UserCredentials>>(
                    null,
                    result.Errors.Select(ErrorFactory.IdentityError));
            }
            result = await this.userManager.AddToRoleAsync(newUser, user.Role);
            if (!result.Succeeded)
            {
                // TODO: разбор ошибок.
                return new ServiceResult<Tuple<User, UserCredentials>>(
                    null,
                    result.Errors.Select(ErrorFactory.IdentityError));
            }

            var registeredUser = new User(newUser);
            var userCredentials = new UserCredentials(newUser.UserName, user.Role, newUser.InstanceId);

            return new ServiceResult<Tuple<User, UserCredentials>>(
                new Tuple<User, UserCredentials>(registeredUser, userCredentials));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> LoginAsync(
            UsernamePassword usernamePassword)
        {
            var username = usernamePassword.Username;
            var password = usernamePassword.Password;

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
            }

            var role = roleResult.Result;
            var registeredUser = new User(user);
            var userCredentials = new UserCredentials(user.UserName, role, user.InstanceId);
            return new ServiceResult<Tuple<User, UserCredentials>>(
                new Tuple<User, UserCredentials>(registeredUser, userCredentials));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Tuple<User, UserCredentials>>> LoginWithRegistrationAsync(
            UsernamePassword usernamePassword)
        {
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var loginResult = await this.LoginAsync(usernamePassword);
                    if (loginResult.Success)
                    {
                        transaction.Commit();
                        return loginResult;
                    }
                    
                    InvitationModel invitationModel = null;
                    foreach (var error in loginResult.Errors)
                    {
                        if (error.Code == ErrorCode.UserNotFound
                            && this.TryGetInvitaiton(usernamePassword.Username, out invitationModel))
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
                        || !this.TryGetInvitaiton(usernamePassword.Username, out var invitation))
                    {
                        // Несмотря на ошибку, никаких изменений не произведено.
                        transaction.Commit();
                        return new ServiceResult<Tuple<User, UserCredentials>>(
                            ErrorFactory.UserNotFound(usernamePassword.Username));
                    }

                    var newUser = new User
                    {
                        Username = usernamePassword.Username,
                        Surname = invitation.Surname,
                        Name = invitation.Name,
                        PhoneNumber = invitation.PhoneNumber,
                        Instance = new Instance(invitation.Instance),
                        Role = invitation.Role.Name,
                    };

                    this.dbContext.Invitations.Remove(invitation);
                    var registrationResult = await this.RegisterAsync(newUser, usernamePassword);
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
            }
            return new ServiceResult<Tuple<User, UserCredentials>>(ErrorFactory.ServerError());
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult> EditAsync(string username, User newData)
        {
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
            }

            return new ServiceResult();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ValidatePasswordAsync(UsernamePassword usernamePassword)
        {
            var userResult = await this.FindUserAsync(usernamePassword.Username);
            if (!userResult.Success)
            {
                return new ServiceResult(userResult.Errors);
            }
            if (!await this.userManager.CheckPasswordAsync(userResult.Result, usernamePassword.Password))
            {
                return new ServiceResult(ErrorFactory.AccessDenied());
            }
            return new ServiceResult();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ChangePasswordAsync(
            string username, 
            string oldPassword,
            string newPassword)
        {
            var userResult = await this.FindUserAsync(username);
            if (!userResult.Success)
            {
                return new ServiceResult(userResult.Errors);
            }

            var user = userResult.Result;

            var result = await this.userManager.ChangePasswordAsync(
                user,
                oldPassword,
                newPassword);
            // TODO: обработка IdentityError
            return result.Succeeded
                ? new ServiceResult()
                : new ServiceResult(result.Errors.Select(ErrorFactory.IdentityError));
        }
    }
}