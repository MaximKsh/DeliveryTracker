using System;
using System.Linq;
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
    [Route("api/session")]
    public class SessionController: Controller
    {
        #region fields

        private readonly DeliveryTrackerDbContext dbContext;
        
        private readonly AccountService accountService;

        private readonly ILogger<SessionController> logger;
        
        #endregion
        
        #region constuctor
        
        public SessionController(
            DeliveryTrackerDbContext dbContext,
            AccountService accountService, 
            ILogger<SessionController> logger)
        {
            this.dbContext = dbContext;
            this.accountService = accountService;
            this.logger = logger;
        }

        #endregion
        
        #region actions
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CredentialsViewModel credentials)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("username", credentials, p => p?.Username != null)
                .AddRule("password", credentials, p => p?.Password != null)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
            }
            
            var loginResult = await this.accountService.Login(credentials);
            // Залогинили - отдаем токен
            if (loginResult.Success)
            {
                if (!string.IsNullOrWhiteSpace(credentials.Device?.FirebaseId))
                {
                    await this.accountService.UpdateUserDevice(
                        credentials.Username, 
                        credentials.Device);
                    await this.dbContext.SaveChangesAsync();
                }
                return this.Ok(loginResult.Result);
            }
            // Не залогинили - ошибка не связана с отсутствием юзера, или юзер отсутствует,
            // но приглашения для него тоже нет
            if (loginResult.Errors.All(p => p.Code != ErrorCode.UserNotFound) 
                || !this.accountService.TryGetInvitaiton(credentials.Username, out var invitation))
            {
                return this.Unauthorized();
            }
            // Транзакция нужна, т.к. происходит регистрация внутри которой данные меняются не только через dbSaveChanges.
            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // пробуем пригласить юзера
                    var acceptInvitationResult = await this.accountService.RegisterWithInvitation(invitation, credentials);
                    if (!acceptInvitationResult.Success)
                    {
                        transaction.Rollback();
                        return this.Unauthorized();
                    }
                    loginResult = await this.accountService.Login(credentials);
                    if (!loginResult.Success)
                    {
                        transaction.Rollback();
                        return this.Unauthorized();
                    }
                    if (!string.IsNullOrWhiteSpace(credentials.Device?.FirebaseId))
                    {
                        await this.accountService.UpdateUserDevice(
                            credentials.Username, 
                            credentials.Device);
                    }
                    await this.dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return this.StatusCode(201, loginResult.Result);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        [Authorize]
        [HttpGet("check")]
        public async Task<IActionResult> CheckSession()
        {
            var currentUserResult = await this.accountService.FindUser(this.User.Identity.Name);
            if (!currentUserResult.Success)
            {
                return this.Unauthorized();
            }
            var user = currentUserResult.Result;
            
            var roleResult = await this.accountService.GetUserRole(user);
            if (!roleResult.Success)
            {
                return this.Unauthorized();
            }
            var role = roleResult.Result;
            var position = user.Latitude.HasValue && user.Longitude.HasValue
                ? new GeopositionViewModel {Latitude = user.Latitude.Value, Longitude = user.Longitude.Value}
                : null;
            
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
                Role = role,
                Position = position, 
            });
        }

        [Authorize]
        [HttpPost("update_device")]
        public async Task<IActionResult> UpdateDevice([FromBody] DeviceViewModel device)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("device", device, p => !string.IsNullOrWhiteSpace(p?.FirebaseId))
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
            }
            
            var updateDeviceResult = 
                await this.accountService.UpdateUserDevice(this.User.Identity.Name, device);
            if (!updateDeviceResult.Success)
            {
                return this.BadRequest(updateDeviceResult.Errors.ToErrorListViewModel());
            }

            return this.Ok();
        }
        
        #endregion
        
    }
}