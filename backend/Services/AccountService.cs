using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DeliveryTracker.Auth;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
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
        /// Регистрация нового пользователя. 
        /// Username сгенерирован автоматически.
        /// </summary>
        /// <param name="displayableName"></param>
        /// <param name="password"></param>
        /// <param name="roleName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        /// <exception cref="AccountServiceException"></exception>
        public async Task<UserModel> Register(
            string displayableName,
            string password,
            string roleName,
            GroupModel group)
        {
            var newUser = new UserModel
            {
                UserName = GenerateInvitationCode(),
                DisplayableName = displayableName,
                Group = group,
            };
            return await this.RegisterInternal(newUser, password, roleName);
        }
        
        /// <summary>
        /// Генерация нового пользователя с указанным username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="displayableName"></param>
        /// <param name="password"></param>
        /// <param name="roleName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        /// <exception cref="AccountServiceException"></exception>
        public async Task<UserModel> Register(
            string username,
            string displayableName,
            string password,
            string roleName,
            GroupModel group)
        {
            var newUser = new UserModel
            {
                UserName = username,
                DisplayableName = displayableName,
                Group = group,
            };
            return await this.RegisterInternal(newUser, password, roleName);
        }
        
        /// <summary>
        /// Регистрация нового пользователя. 
        /// Username сгенерирован автоматически.
        /// </summary>
        /// <param name="displayableName"></param>
        /// <param name="password"></param>
        /// <param name="roleName"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        /// <exception cref="AccountServiceException"></exception>
        public async Task<UserModel> Register(
            string displayableName,
            string password,
            string roleName,
            Guid groupId)
        {
            var newUser = new UserModel
            {
                UserName = GenerateInvitationCode(),
                DisplayableName = displayableName,
                GroupId = groupId,
            };
            return await this.RegisterInternal(newUser, password, roleName);
        }

        /// <summary>
        /// Генерация нового пользователя с указанным username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="displayableName"></param>
        /// <param name="password"></param>
        /// <param name="roleName"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        /// <exception cref="AccountServiceException"></exception>
        public async Task<UserModel> Register(
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
                GroupId = groupId,
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
        public async Task<LoginResponseViewModel> Login(
            string username,
            string password,
            string expectingRole)
        {
            var user = await this.userManager.FindByNameAsync(username);
            if (!await this.userManager.CheckPasswordAsync(user, password))
            {
                return null;
            }
            var userRoles = await this.userManager.GetRolesAsync(user);
            var role = userRoles.FirstOrDefault(p => p == expectingRole);
            if (role == null)
            {
                return null;
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
                signingCredentials: new SigningCredentials(AuthHelper.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new LoginResponseViewModel
            {
                DisplayableName = user.DisplayableName,
                Token = encodedJwt,
                Role = role,
            };
        }

        /// <summary>
        /// Создать приглашение для роли.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<InvitationModel> CreateInvitation(Guid groupId, Guid roleId)
        {
            var invitation = new InvitationModel
            {
                Id = Guid.NewGuid(),
                InvitationCode = GenerateInvitationCode(),
                ExpirationDate = DateTime.UtcNow.AddDays(InvitationExpirationPeriodInDays),
                RoleId = roleId,
                GroupId = groupId,
            };

            await this.dbContext.Invitations.AddAsync(invitation);
            await this.dbContext.SaveChangesAsync();

            return invitation;
        }

        /// <summary>
        /// Принять приглашение.
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>true - если приглашение существовало, false - в противном случае</returns>
        public async Task<UserModel> AcceptInvitation(
            string invitationCode,
            string username, 
            string password)
        {
            var invitation = await this.dbContext
                .Invitations
                .Include(p => p.Role)
                .Include(p => p.Group)
                .FirstOrDefaultAsync(p => p.InvitationCode == invitationCode);
            if (invitation == null)
            {
                return null;
            }
            var roleName = invitation.Role.Name;
            var group = invitation.Group;
            this.dbContext.Invitations.Remove(invitation);
            var newUser = await this.Register(
                invitationCode,
                username,
                password,
                roleName,
                group);
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
        
        
        public async Task<UserModel> RegisterInternal(
            UserModel newUser,
            string password,
            string roleName)
        {
            var result = await this.userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                throw new AccountServiceException(result.Errors);
            }
            result = await this.userManager.AddPasswordAsync(newUser, password);
            if (!result.Succeeded)
            {
                throw new AccountServiceException(result.Errors);
            }
            result = await this.userManager.AddToRoleAsync(newUser, roleName);
            if (!result.Succeeded)
            {
                throw new AccountServiceException(result.Errors);
            }
            return newUser;
        }
        
        
        
        #endregion
       
    }
}