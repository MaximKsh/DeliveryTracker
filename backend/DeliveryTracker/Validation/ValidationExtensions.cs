using System;
using System.Collections.Generic;
using System.Linq;

namespace DeliveryTracker.Validation
{
    public static class ValidationExtensions
    {
        public static string ErrorsToString(this IEnumerable<IError> errors)
        {
            return string.Join(Environment.NewLine, errors.Select(e => e.ToString()));
        }
    }
}