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
    [Route("api/group")]
    public sealed class GroupController: Controller
    {
        #region fields
        
        private readonly DeliveryTrackerDbContext dbContext;
        
        private readonly AccountService accountService;
        
        private readonly RoleCache roleCache;
        
        private readonly GroupService groupService;
        
        private readonly ILogger<GroupController> logger;
        
        #endregion
        
        #region constructor
        
        public GroupController(
            DeliveryTrackerDbContext dbContext, 
            AccountService accountService,
            RoleCache roleCache,
            GroupService groupService, 
            ILogger<GroupController> logger)
        {
            this.dbContext = dbContext;
            this.accountService = accountService;
            this.roleCache = roleCache;
            this.groupService = groupService;
            this.logger = logger;
        }

        #endregion 
        
        #region actions
        
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateGroupViewModel groupViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }
            
            // Регистрацию необходмо выполнять в транзакции.
            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var groupResult = this.groupService.CreateGroup(groupViewModel.GroupName);
                    if (!groupResult.Success)
                    {
                        transaction.Rollback();
                        return this.BadRequest(groupResult.Errors.ToErrorListViewModel());
                    }
                    var group = groupResult.Result;
                    
                    var userResult = await this.accountService.Register(
                        groupViewModel.CreatorDisplayableName,
                        groupViewModel.CreatorPassword,
                        this.roleCache.Creator.Name,
                        group.Id);
                    if (!userResult.Success)
                    {
                        transaction.Rollback();
                        return this.BadRequest(userResult.Errors.ToErrorListViewModel());
                    }
                    var user = userResult.Result;
                    
                    var setCreatorResult = this.groupService.SetCreator(group, user.Id);
                    if (!setCreatorResult.Success)
                    {
                        transaction.Rollback();
                        return this.BadRequest(setCreatorResult.Errors.ToErrorListViewModel());
                    }
                    
                    await this.dbContext.SaveChangesAsync();
                    transaction.Commit();
                    
                    return this.Ok(new UserInfoViewModel
                    {
                        UserName = user.UserName,
                        DisplayableName = user.DisplayableName,
                        Group = group.DisplayableName,
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
        [HttpGet("invite_manager")]
        public async Task<IActionResult> InviteManager()
        {
            var currentUserResult = await this.accountService.FindUser(this.User.Identity.Name);
            if (!currentUserResult.Success)
            {
                return this.NotFound(currentUserResult.Errors.First());
            }
            var currentUser = currentUserResult.Result;
                
            var invitationResult = this.accountService.CreateInvitation(
                currentUser.GroupId,
                this.roleCache.Manager.Id);
            if (!invitationResult.Success)
            {
                return this.BadRequest(invitationResult.Errors.ToErrorListViewModel());
            }
            var invitation = invitationResult.Result;

            await this.dbContext.SaveChangesAsync();
            return this.Ok(new InvitationViewModel
            {
                InvitationCode = invitation.InvitationCode,
                Role = this.roleCache.Manager.Name,
                ExpirationDate = invitation.ExpirationDate,
                GroupName = currentUser.Group.DisplayableName,
            });
        }
        
        [Authorize(Policy = AuthPolicies.CreatorOrManager)]
        [HttpGet("invite_performer")]
        public async Task<IActionResult> InvitePerformer()
        {
            var currentUserResult = await this.accountService.FindUser(this.User.Identity.Name);
            if (!currentUserResult.Success)
            {
                return this.NotFound(currentUserResult.Errors.First());
            }
            var currentUser = currentUserResult.Result;
                
            var invitationResult = this.accountService.CreateInvitation(
                currentUser.GroupId,
                this.roleCache.Performer.Id);
            if (!invitationResult.Success)
            {
                return this.BadRequest(invitationResult.Errors.ToErrorListViewModel());
            }
            var invitation = invitationResult.Result;
            await this.dbContext.SaveChangesAsync();
            return this.Ok(new InvitationViewModel
            {
                InvitationCode = invitation.InvitationCode,
                Role = this.roleCache.Performer.Name,
                ExpirationDate = invitation.ExpirationDate,
                GroupName = currentUser.Group.DisplayableName,
            });
        }
        
        [HttpPost("accept_invitation")]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationViewModel acceptInvitation)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }
            
            // Транзакция нужна, т.к. юзер менеджер в себе выполняет сохранения 
            // и их нужно откатывать
            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (!this.accountService.TryGetInvitaiton(
                            acceptInvitation.InvitationCode,
                            out var invitation))
                    {
                        return this.NotFound(
                            ErrorFactory.InvitationDoesNotExist(acceptInvitation.InvitationCode));
                    }
                    var newUserResult = await this.accountService.AcceptInvitation(
                            invitation,
                            acceptInvitation.DisplayableName,
                            acceptInvitation.Password);
                    if (!newUserResult.Success)
                    {
                        return this.BadRequest(newUserResult.Errors.ToErrorListViewModel());
                    }
                    var newUser = newUserResult.Result;
                    await this.dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return this.Ok(new UserInfoViewModel
                    {
                        UserName = newUser.UserName,
                        DisplayableName = newUser.DisplayableName,
                        Group = newUser.Group.DisplayableName,
                        Role = invitation.Role.Name,
                    });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        
        #endregion
    }

}