using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DeliveryTracker.Auth;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
using DeliveryTracker.Roles;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryTracker.Services
{
    public class AccountService
    {
        #region constants

        private const int InvitationExpirationPeriodInDays = 3;

        private const int InvitationCodeLength = 10;

        private static readonly char[] InvitationCodeAlphabet =
            "23456789qwertyupasdfghkzxbnmQWERTYUPASDFGHKZXVBNM".ToCharArray();

        private static readonly Random Random = new Random();

        #endregion

        #region fields

        private readonly UserManager<UserModel> userManager;
        
        private readonly DeliveryTrackerDbContext dbContext;

        #endregion

        #region constuctor

        public AccountService(
            UserManager<UserModel> userManager,
            DeliveryTrackerDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        #endregion

        #region public

        /// <summary>
        /// Загрузить пользователя по имени.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="withGroup"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> FindUser(string username, bool withGroup = true)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new ServiceResult<UserModel>(
                    null,
                    ErrorFactory.UserNotFound(username));
            }
            var currentUser = await this.userManager.FindByNameAsync(username);
            if (currentUser == null)
            {
                return new ServiceResult<UserModel>(
                    null,
                    ErrorFactory.UserNotFound(username));
            }
            if (withGroup)
            {
                this.dbContext.Entry(currentUser).Reference(p => p.Instance).Load();
            }
            return new ServiceResult<UserModel>(currentUser);
        }

        /// <summary>
        /// Получить роль пользователя.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ServiceResult<string>> GetUserRole(UserModel user)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }

            var role = (await this.userManager.GetRolesAsync(user))
                .FirstOrDefault(p => p == RoleInfo.Creator || p == RoleInfo.Manager || p == RoleInfo.Performer);
            
            return role != null 
                ? new ServiceResult<string>(role) 
                : new ServiceResult<string>(null, ErrorFactory.UserWithoutRole(user.UserName));
        }

        /// <summary>
        /// Регистрация нового пользователя. Необходимо выполнять в транзакции.
        /// Username сгенерирован автоматически.
        /// </summary>
        /// <param name="displayableName"></param>
        /// <param name="password"></param>
        /// <param name="roleName"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> Register(
            string displayableName,
            string password,
            string roleName,
            Guid groupId)
        {
            var newUser = new UserModel
            {
                UserName = GenerateInvitationCode(),
                DisplayableName = displayableName,
                InstanceId = groupId,
            };
            return await this.RegisterInternal(newUser, password, roleName);
        }

        /// <summary>
        /// Создание нового пользователя с указанным username. Необходимо выполнять в транзакции.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="displayableName"></param>
        /// <param name="password"></param>
        /// <param name="roleName"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> Register(
            string username,
            string displayableName,
            string password,
            string roleName,
            Guid groupId)
        {
            var newUser = new UserModel
            {
                UserName = username,
                DisplayableName = displayableName,
                InstanceId = groupId,
            };
            return await this.RegisterInternal(newUser, password, roleName);
        }

        /// <summary>
        /// Выписывает токен для пользователя.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="expectingRole"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TokenViewModel>> Login(
            string username,
            string password,
            string expectingRole)
        {
            var userResult = await this.FindUser(username);
            if (!userResult.Success
                || userResult.Result == null
                || !await this.userManager.CheckPasswordAsync(userResult.Result, password))
            {
                return new ServiceResult<TokenViewModel>(
                    null,
                    ErrorFactory.UserNotFound(username));
            }
            var user = userResult.Result;
            var roleResult = await this.GetUserRole(user);
            if (!roleResult.Success)
            {
                return new ServiceResult<TokenViewModel>(
                    null,
                    ErrorFactory.UserNotInRole(user.UserName, expectingRole));
            }
            var role = roleResult.Result;
            if (role != expectingRole)
            {
                return new ServiceResult<TokenViewModel>(
                    null,
                    ErrorFactory.UserNotInRole(user.UserName, expectingRole));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
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
                issuer: AuthHelper.Issuer,
                audience: AuthHelper.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.AddMinutes(AuthHelper.Lifetime),
                signingCredentials: new SigningCredentials(
                    AuthHelper.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new ServiceResult<TokenViewModel>(new TokenViewModel
            {
                User = new UserInfoViewModel
                {
                    DisplayableName = user.DisplayableName,
                    Instance = user.Instance.DisplayableName,
                    Role = role,
                    Position = null,
                    UserName = user.UserName,
                },
                Token = encodedJwt,
            });
        }

        /// <summary>
        /// Создать приглашение для роли.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public ServiceResult<InvitationModel> CreateInvitation(Guid groupId, Guid roleId)
        {
            var invitation = new InvitationModel
            {
                Id = Guid.NewGuid(),
                InvitationCode = GenerateInvitationCode(),
                ExpirationDate = DateTime.UtcNow.AddDays(InvitationExpirationPeriodInDays),
                RoleId = roleId,
                InstanceId = groupId,
            };

            this.dbContext.Invitations.Add(invitation);

            return new ServiceResult<InvitationModel>(invitation);
        }

        /// <summary>
        /// Получить объект приглашения по коду. 
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <param name="invitation"></param>
        /// <returns></returns>
        public bool TryGetInvitaiton(string invitationCode, out InvitationModel invitation)
        {
            invitation =  this.dbContext
                .Invitations
                .Include(p => p.Role)
                .Include(p => p.Instance)
                .FirstOrDefault(p => p.InvitationCode == invitationCode);
            return invitation != null;
        }
    
        /// <summary>
        /// Принять приглашение. Необходимо выполнять в транзакции.
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>true - если приглашение существовало, false - в противном случае</returns>
        public async Task<ServiceResult<UserModel>> AcceptInvitation(
            string invitationCode,
            string username, 
            string password)
        {
            if (this.TryGetInvitaiton(invitationCode, out var invitation))
            {
                return await this.AcceptInvitation(invitation, username, password);
            }
            return new ServiceResult<UserModel>(
                null,
                ErrorFactory.InvitationDoesNotExist(invitationCode));
        }
        
        /// <summary>
        /// Принять приглашение. Необходимо выполнять в транзакции.
        /// </summary>
        /// <param name="invitation"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> AcceptInvitation(
            InvitationModel invitation,
            string username, 
            string password)
        {
            if (invitation == null)
            {
                throw new ArgumentNullException();
            }
            if (invitation.ExpirationDate < DateTime.UtcNow)
            {
                return new ServiceResult<UserModel>(
                    null,
                    ErrorFactory.InvitaitonExpired(invitation.InvitationCode));
            }
            var roleName = invitation.Role.Name;
            var group = invitation.Instance;
            this.dbContext.Invitations.Remove(invitation);
            var newUser = await this.Register(
                invitation.InvitationCode,
                username,
                password,
                roleName,
                group.Id);
            return newUser;
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
        
        #endregion
       
    }
}