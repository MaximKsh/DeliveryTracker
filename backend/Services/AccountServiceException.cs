using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace DeliveryTracker.Services
{
    public class AccountServiceException: Exception
    {
        public AccountServiceException(IEnumerable<IdentityError> errors):
            base(string.Join(Environment.NewLine, errors.Select(p => $"{p.Code}: {p.Description}")))
        {
        }
    }
}