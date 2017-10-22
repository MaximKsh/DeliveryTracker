using System;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Auth;
using DeliveryTracker.Db;
using DeliveryTracker.Helpers;
using DeliveryTracker.Roles;
using DeliveryTracker.Services;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Controllers
{
    [Route("api/instance")]
    public sealed class InstanceController: Controller
    {
        #region fields
        
        private readonly DeliveryTrackerDbContext dbContext;
        
        private readonly AccountService accountService;
        
        private readonly RoleCache roleCache;
        
        private readonly InstanceService instanceService;
        
        private readonly ILogger<InstanceController> logger;
        
        #endregion
        
        #region constructor
        
        public InstanceController(
            DeliveryTrackerDbContext dbContext, 
            AccountService accountService,
            RoleCache roleCache,
            InstanceService instanceService, 
            ILogger<InstanceController> logger)
        {
            this.dbContext = dbContext;
            this.accountService = accountService;
            this.roleCache = roleCache;
            this.instanceService = instanceService;
            this.logger = logger;
        }

        #endregion 
        
        #region actions
        
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateInstanceViewModel instanceViewModel)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("instanceInfo", instanceViewModel, p => p != null)
                .AddRule("instanceName", instanceViewModel?.Instance?.InstanceName, p => !string.IsNullOrWhiteSpace(p))
                .AddRule("surname", instanceViewModel?.Creator?.Surname, p => !string.IsNullOrWhiteSpace(p))
                .AddRule("name", instanceViewModel?.Creator?.Name, p => !string.IsNullOrWhiteSpace(p))
                .AddRule("role", instanceViewModel?.Creator?.Role, p => p == this.roleCache.Creator.Name)
                .AddRule("password", instanceViewModel?.Credentials?.Password, p => !string.IsNullOrWhiteSpace(p))
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }
            
            // Регистрацию необходмо выполнять в транзакции.
            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var instanceResult = this.instanceService.CreateInstance(instanceViewModel.Instance);
                    if (!instanceResult.Success)
                    {
                        transaction.Rollback();
                        return this.BadRequest(instanceResult.Errors.ToErrorListViewModel());
                    }
                    var instance = instanceResult.Result;
                    
                    var userResult = await this.accountService.Register(
                        instanceViewModel.Credentials,
                        instanceViewModel.Creator,
                        instance.Id);
                    if (!userResult.Success)
                    {
                        transaction.Rollback();
                        return this.BadRequest(userResult.Errors.ToErrorListViewModel());
                    }
                    var user = userResult.Result;
                    
                    var setCreatorResult = this.instanceService.SetCreator(instance, user.Id);
                    if (!setCreatorResult.Success)
                    {
                        transaction.Rollback();
                        return this.BadRequest(setCreatorResult.Errors.ToErrorListViewModel());
                    }
                    
                    await this.dbContext.SaveChangesAsync();
                    transaction.Commit();
                    
                    return this.Ok(new UserViewModel
                    {
                        Username = user.UserName,
                        Surname = user.Surname,
                        Name = user.Name,
                        PhoneNumber = user.PhoneNumber,
                        Instance = new InstanceViewModel
                        {
                            InstanceName = user.Instance.InstanceName,
                        },
                        Role = this.roleCache.Creator.Name, 
                    });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        [Authorize(Policy = AuthPolicies.Creator)]
        [HttpPost("invite_manager")]
        public async Task<IActionResult> InviteManager([FromBody] UserViewModel preliminaryUserInfo = null)
        {
            var invitationResult = await this.accountService.CreateInvitation(
                this.User.Identity.Name,
                this.roleCache.Manager,
                preliminaryUserInfo);
            if (!invitationResult.Success)
            {
                return this.BadRequest(invitationResult.Errors.ToErrorListViewModel());
            }
            var invitation = invitationResult.Result;

            await this.dbContext.SaveChangesAsync();
            return this.Ok(new InvitationViewModel
            {
                InvitationCode = invitation.InvitationCode,
                ExpirationDate = invitation.ExpirationDate,
                PreliminaryUser = new UserViewModel
                {
                    Surname = invitation.Surname,
                    Name = invitation.Name,
                    PhoneNumber = invitation.PhoneNumber,
                }
            });
        }
        
        [Authorize(Policy = AuthPolicies.CreatorOrManager)]
        [HttpPost("invite_performer")]
        public async Task<IActionResult> InvitePerformer([FromBody] UserViewModel preliminaryUserInfo = null)
        { 
            var invitationResult = await this.accountService.CreateInvitation(
                this.User.Identity.Name,
                this.roleCache.Performer,
                preliminaryUserInfo);
            if (!invitationResult.Success)
            {
                return this.BadRequest(invitationResult.Errors.ToErrorListViewModel());
            }
            var invitation = invitationResult.Result;

            await this.dbContext.SaveChangesAsync();
            return this.Ok(new InvitationViewModel
            {
                InvitationCode = invitation.InvitationCode,
                ExpirationDate = invitation.ExpirationDate,
                PreliminaryUser = new UserViewModel
                {
                    Surname = invitation.Surname,
                    Name = invitation.Name,
                    PhoneNumber = invitation.PhoneNumber,
                }
            });
        }

        [Authorize]
        [HttpGet("performers")]
        public async Task<IActionResult> GetPerformers(int? limitParam, int? offsetParam)
        {
            return await this.GetUsers(limitParam, offsetParam, true);
        }
        
        [Authorize]
        [HttpGet("managers")]
        public async Task<IActionResult> GetManagers(int? limitParam, int? offsetParam)
        {
            return await this.GetUsers(limitParam, offsetParam, false);
        }

        [Authorize(Policy = AuthPolicies.CreatorOrManager)]
        [HttpGet("delete_performer")]
        public async Task<IActionResult> DeletePerformer([FromBody] UserViewModel userInfo)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("user", userInfo, p => p != null)
                .AddRule("username", userInfo?.Username, p => p != null)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }

            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var result = await this.instanceService.DeletePerformer(this.User.Identity.Name, userInfo.Username);
                    if (!result.Success)
                    {
                        // Обработаем первую ошибку
                        var error = result.Errors[0];
                        transaction.Rollback();
                        if (error.Code == ErrorCode.UserNotFound)
                        {
                            return this.NotFound(error.ToErrorListViewModel());
                        }
                        if (error.Code == ErrorCode.AccessDenied)
                        {
                            return this.StatusCode(403, error.ToErrorListViewModel());
                        }
                        return this.BadRequest(error.ToErrorListViewModel());
                    }
                    await this.dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return this.Ok();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            
        }
        
        [Authorize(Policy = AuthPolicies.Creator)]
        [HttpGet("delete_manager")]
        public async Task<IActionResult> DeleteManager([FromBody] UserViewModel userInfo)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("user", userInfo, p => p != null)
                .AddRule("number", userInfo?.Username, p => p != null)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }
            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var result = await this.instanceService.DeleteManager(this.User.Identity.Name, userInfo.Username);
                    if (!result.Success)
                    {
                        // Обработаем первую ошибку
                        var error = result.Errors[0];
                        transaction.Rollback();
                        if (error.Code == ErrorCode.UserNotFound)
                        {
                            return this.NotFound(error.ToErrorListViewModel());
                        }
                        if (error.Code == ErrorCode.AccessDenied)
                        {
                            return this.StatusCode(403, error.ToErrorListViewModel());
                        }
                        return this.BadRequest(error.ToErrorListViewModel());
                    }
                    await this.dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return this.Ok();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        
        #endregion
        
        #region private

        private async Task<IActionResult> GetUsers(int? limitParam, int? offsetParam, bool performers)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("limit", limitParam, p => p == null || p <= 0)
                .AddRule("offset", offsetParam, p => p == null || p < 0)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }
            var limit = limitParam ?? 100;
            var offset = offsetParam ?? 0;
            var result = performers
                ? await this.instanceService.GetPerformers(this.User.Identity.Name, offset, limit)
                : await this.instanceService.GetManagers(this.User.Identity.Name, offset, limit);
            if (!result.Success)
            {
                // Обработаем первую ошибку
                var error = result.Errors.First();
                if (error.Code == ErrorCode.UserNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                if (error.Code == ErrorCode.UserNotInRole)
                {
                    return this.StatusCode(403, error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }
            var users = result.Result
                .Select(p =>
                    new UserViewModel
                    {
                        Username = p.UserName,
                        Surname = p.Surname,
                        Name = p.Name,
                        PhoneNumber = p.PhoneNumber,
                        Role = this.roleCache.Performer.Name
                    });
            return this.Ok(users);
        }
        
        #endregion
    }

}