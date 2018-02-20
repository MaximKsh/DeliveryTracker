using System;

namespace DeliveryTracker.Identification
{
    public sealed class Session
    {
        public Guid UserId { get; set; }
        
        public Guid? SessionTokenId { get; set; }
        
        public string SessionToken { get; set; }
        
        public Guid? RefreshTokenId { get; set; }
        
        public string RefreshToken { get; set; }
        
        public DateTime? LastActivity { get; set; }
        
    }
}