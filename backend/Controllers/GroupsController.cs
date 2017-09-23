using System;
using System.Threading.Tasks;
using backend.ViewModels.Errors;
using DeliveryTracker.Auth;
using DeliveryTracker.Caching;
using DeliveryTracker.Db;
using DeliveryTracker.Helpers;
using DeliveryTracker.Models;
using DeliveryTracker.Services;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTracker.Controllers
{
    public sealed class GroupsController: Controller
    {
        #region fields
        
        private readonly DeliveryTrackerDbContext dbContext;
        private readonly AccountService accountService;
        private readonly UserManager<UserModel> userManager;
        private readonly RolesCache rolesCache;
        private readonly GroupService groupService;
        
        #endregion
        
        #region constructor
        
        public GroupsController(
            DeliveryTrackerDbContext dbContext, 
            AccountService accountService,
            UserManager<UserModel> userManager,
            RolesCache rolesCache, GroupService groupService)
        {
            this.dbContext = dbContext;
            this.accountService = accountService;
            this.userManager = userManager;
            this.rolesCache = rolesCache;
            this.groupService = groupService;
        }

        #endregion 
        
        #region actions
        
        [HttpPost("/groups/create")]
        public async Task<IActionResult> Create([FromBody] CreateGroupRequestViewModel groupViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }
            
            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var group = await this.groupService.CreateGroup(groupViewModel.GroupName);
                    var user = await this.accountService.Register(
                        groupViewModel.CreatorDisplayableName,
                        groupViewModel.CreatorPassword,
                        this.rolesCache.Creator.Name,
                        group.Id);
                    this.groupService.SetCreator(group, user);
                    await this.dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return this.Ok(new RegisterResponseViewModel
                    {
                        UserName = user.UserName,
                        DisplayableName = user.DisplayableName,
                        Group = group.DisplayableName,
                    });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return this.BadRequest($"{e.Message}");
                }
            }
        }
        
        [Authorize(Policy = AuthPolicies.Creator)]
        [HttpGet("/groups/invite_manager")]
        public async Task<IActionResult> InviteManager()
        {
            var currentUser = await this.userManager.FindByNameAsync(this.User.Identity.Name);
            var invitation = await this.accountService.CreateInvitation(
                currentUser.GroupId, 
                this.rolesCache.Manager.Id);
            return this.Ok(new InvitationResponseViewModel
            {
                InvitationCode = invitation.InvitationCode,
                Role = this.rolesCache.Manager.Name,
                ExpirationDate = invitation.ExpirationDate,
            });
        }
        
        [Authorize(Policy = AuthPolicies.CreatorOrManager)]
        [HttpGet("/groups/invite_performer")]
        public async Task<IActionResult> InvitePerformer()
        {
            var currentUser = await this.userManager.FindByNameAsync(this.User.Identity.Name);
            var invitation = await this.accountService.CreateInvitation(
                currentUser.GroupId, 
                this.rolesCache.Performer.Id);
            return this.Ok(new InvitationResponseViewModel
            {
                InvitationCode = invitation.InvitationCode,
                Role = this.rolesCache.Performer.Name,
                ExpirationDate = invitation.ExpirationDate,
            });
        }
        
        [HttpPost("/groups/accept_invitation")]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequestViewModel acceptInvitation)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }

            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var newUser = await this.accountService.AcceptInvitation(
                        acceptInvitation.InvitationCode,
                        acceptInvitation.DisplayableName,
                        acceptInvitation.Password);
                    if (newUser != null)
                    {
                        await this.dbContext.SaveChangesAsync();
                        transaction.Commit();
                        return this.Ok(new RegisterResponseViewModel
                        {
                            UserName = newUser.UserName,
                            DisplayableName = newUser.DisplayableName,
                            Group = newUser.Group.DisplayableName,
                        });
                    }
                    else
                    {
                        transaction.Rollback();
                        return this.BadRequest(new InvalidInvitationCodeViewModel());
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return this.BadRequest($"{e.Message}");
                }
            }
        }
        
        #endregion
    }

}