using DeliveryTracker.Instances;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/instance")]
    public sealed class InstanceController: Controller
    {
        #region fields
        
        
        private readonly AccountService accountService;
        
        
        private readonly InstanceService instanceService;
        
        private readonly ILogger<InstanceController> logger;
        
        #endregion
        
        #region constructor
        
        public InstanceController(
            AccountService accountService,
            InstanceService instanceService, 
            ILogger<InstanceController> logger)
        {
            this.accountService = accountService;
            this.instanceService = instanceService;
            this.logger = logger;
        }

        #endregion 
        
        #region actions
        
        // instance/create
        // instance/settings
        // ...

        // user/invitation/create
        // user/invitation/get
        // user/invitation/delete
        // user/get
        // user/edit
        // user/delete
        // user/update_position
        
        // task/create
        // task/get
        // task/edit
        // task/change_state
        // task/delete
        
        // account/login
        // account/about
        // account/edit
        // account/change_password
        
        // reference/types
        // reference/{type}/create
        // reference/{type}/edit
        // reference/{type}/get
        // reference/{type}/delete
        
        // view/groups
        // view/{groupName}/views
        // view/{groupName}/{viewName}

        [HttpPost("create")]
        public IActionResult Create()
        {
            return this.Ok();
        }
        
        /*
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
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
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
        
        [Authorize]
        [HttpGet("get_user/{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("username", username, p => p != null && !string.IsNullOrWhiteSpace(p))
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
            }
            var currentUserResult = await this.accountService.FindUser(this.User.Identity.Name, false);
            if (!currentUserResult.Success)
            {
                return this.NotFound(ErrorFactory.UserNotFound(this.User.Identity.Name).ToErrorListViewModel());
            }
            var userResult = await this.accountService.FindUser(username);
            if (!userResult.Success
                || userResult.Result.InstanceId != currentUserResult.Result.InstanceId)
            {
                return this.NotFound(ErrorFactory.UserNotFound(username).ToErrorListViewModel());
            }
            var user = userResult.Result;
            var role = await this.accountService.GetUserRole(user);
            return this.Ok(new UserViewModel
            {
                Instance = new InstanceViewModel
                {
                    InstanceName = user.Instance.InstanceName
                },
                Username = user.UserName,
                Surname = user.Surname,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Role = role.Result ?? string.Empty,
                
            });
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
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
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
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
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
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
            }
            var limit = limitParam ?? 100;
            var offset = offsetParam ?? 0;
            var result = performers
                ? await this.instanceService.GetPerformers(this.User.Identity.Name, limit, offset)
                : await this.instanceService.GetManagers(this.User.Identity.Name, limit, offset);

            UserModel creator = null;
            if (!performers)
            {
                // TODO: переписать нормально, чтобы для менеджеров в выборку попадал создатель
                // Но пока тоже нормально, юзер должен лежать в кэше ef
                var currentUserResult = await this.accountService.FindUser(this.User.Identity.Name);
                if (!currentUserResult.Success)
                {
                    return this.BadRequest(currentUserResult.Errors);
                }
                var currentInstance = currentUserResult.Result.Instance;
                var creatorResult = await this.instanceService.GetCreator(currentInstance);
                if (!creatorResult.Success)
                {
                    return this.BadRequest(creatorResult.Errors);
                }
                creator = creatorResult.Result;
            }
            
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
                        Role = performers ? this.roleCache.Performer.Name : this.roleCache.Manager.Name
                    })
                .ToList();

            if (creator != null)
            {
                // TODO: вставка в начало - грустно
                users.Insert(
                    0, 
                    new UserViewModel
                    {
                        Username = creator.UserName,
                        Surname = creator.Surname,
                        Name = creator.Name,
                        PhoneNumber = creator.PhoneNumber,
                        Role = this.roleCache.Creator.Name
                    });
            }
            return this.Ok(users);
        }
        */
        #endregion
    }

}