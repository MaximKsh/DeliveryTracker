﻿using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public interface IAccountService
    {
        Task<ServiceResult<Tuple<User, UserCredentials>>> RegisterAsync(
            CodePassword codePassword,
            Action<User> userModificationAction = null,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<Tuple<User, UserCredentials>>> LoginAsync(
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<Tuple<User, UserCredentials>>> LoginWithRegistrationAsync(
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<User>> GetAsync(NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult<User>> EditAsync(
            User newData,
            NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult> ChangePasswordAsync(
            string oldPassword,
            string newPassword,
            NpgsqlConnectionWrapper oc = null);
    }
}