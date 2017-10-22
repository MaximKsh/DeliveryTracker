using System;
using System.Threading.Tasks;
using DeliveryTracker.Db;
using DeliveryTracker.Helpers;
using DeliveryTracker.Services;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Controllers
{
    [Authorize]
    [Route("api/user")]
    public class UserController: Controller
    {
        #region fields
        
        private readonly DeliveryTrackerDbContext dbContext;
        
        private readonly AccountService accountService;
        
        private readonly ILogger<UserController> logger;
        
        #endregion
        
        #region constructor
        
        public UserController(
            DeliveryTrackerDbContext dbContext, 
            AccountService accountService,
            ILogger<UserController> logger)
        {
            this.dbContext = dbContext;
            this.accountService = accountService;
            this.logger = logger;
        }

        #endregion 
        
        #region actions
        
        

        [HttpPost("modify")]
        public async Task<IActionResult> Modify([FromBody] UserViewModel userInfo)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("userInfo", userInfo, p => p != null)
                .Validate();
            
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }
            
            
            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await this.accountService.Modify(this.User.Identity.Name, userInfo);
                    if (!result.Success)
                    {
                        // Обработаем первую ошибку
                        var error = result.Errors[0];
                        transaction.Rollback();
                        if (error.Code == ErrorCode.UserNotFound)
                        {
                            return this.NotFound(error.ToErrorListViewModel());
                        }
                        return this.BadRequest(error.ToErrorListViewModel());
                    }
                    var user = result.Result;
                    var role = await this.accountService.GetUserRole(user);
                    await this.dbContext.SaveChangesAsync();
                    transaction.Commit();
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
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        
        [HttpPost("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel passwords)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("passwords", passwords, p => p != null)
                .AddRule("currentCredentials", passwords.CurrentCredentials, p => p != null)
                .AddRule("newCredentials", passwords.NewCredentials, p => p != null)
                .Validate();
            
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }
            
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }
            
            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await this.accountService.ChangePassword(this.User.Identity.Name, passwords);
                    if (!result.Success)
                    {
                        // Обработаем первую ошибку
                        var error = result.Errors[0];
                        transaction.Rollback();
                        if (error.Code == ErrorCode.UserNotFound)
                        {
                            return this.NotFound(error.ToErrorListViewModel());
                        }
                        return this.BadRequest(error.ToErrorListViewModel());
                    }
                    var user = result.Result;
                    var role = await this.accountService.GetUserRole(user);
                    await this.dbContext.SaveChangesAsync();
                    transaction.Commit();
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