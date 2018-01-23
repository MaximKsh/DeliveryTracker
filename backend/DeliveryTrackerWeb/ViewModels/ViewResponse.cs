﻿using System.Collections.Generic;
using DeliveryTracker.Validation;

namespace DeliveryTrackerWeb.ViewModels
{
    public class ViewResponse : ResponseBase
    {
        public ViewResponse() : base()
        {
        }

        public ViewResponse(IError error) : base(error)
        {
        }

        public ViewResponse(IReadOnlyList<IError> errors) : base(errors)
        {
        }
        
        public string[] Groups { get; set; }
        
        public string[] Views { get; set; }
        
        public IDictionary<string, long> Digest { get; set; }
        
        public IEnumerable<IDictionary<string, object>> ViewResult { get; set; }
    }
}