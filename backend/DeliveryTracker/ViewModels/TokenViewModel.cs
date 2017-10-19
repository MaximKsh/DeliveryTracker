using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class TokenViewModel
    {
        /// <summary>
        /// Пользователь, связанный с токеном.
        /// </summary>
        public UserViewModel User { get; set; }
        
        /// <summary>
        /// JWT токен.
        /// </summary>
        public string Token { get; set; }
    }
}