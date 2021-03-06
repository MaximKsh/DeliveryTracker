﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;

namespace DeliveryTrackerWeb.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class AccountResponse : ResponseBase
    {
        public AccountResponse() : base()
        {
        }

        public AccountResponse(IError error) : base(error)
        {
        }

        public AccountResponse(IReadOnlyList<IError> errors) : base(errors)
        {
        }
        
        public User User { get; set; }
        
        public string Token { get; set; }
        
        public string RefreshToken { get; set; }
    }
}