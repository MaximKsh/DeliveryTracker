using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryTracker.Auth
{
    /// <summary>
    /// Информация для JWT-токенов
    /// </summary>
    internal static class AuthHelper
    {
        /// <summary>
        /// Приватный ключ для токена
        /// </summary>
        private const string Key = "mysupersecret_secretkey!123";

        /// <summary>
        /// Издатель токена
        /// </summary>
        internal const string Issuer = "DeliveryTracker";

        /// <summary>
        /// Потребитель токена
        /// </summary>
        internal const string Audience = "http://localhost:5000/"; 
        
        /// <summary>
        /// Время жизни токена в минутах
        /// </summary>
        internal const int Lifetime = 1; 
        
        internal static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}