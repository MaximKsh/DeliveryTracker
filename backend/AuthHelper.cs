using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryTracker
{
    /// <summary>
    /// Информация для JWT-токенов
    /// TODO: надо подумать, куда перенести
    /// </summary>
    internal static class AuthHelper
    {
        /// <summary>
        /// Приватный ключ для токена
        /// </summary>
        private const string KEY = "mysupersecret_secretkey!123";

        /// <summary>
        /// Издатель токена
        /// </summary>
        internal const string ISSUER = "DeliveryTracker";

        /// <summary>
        /// Потребитель токена
        /// </summary>
        internal const string AUDIENCE = "http://localhost:5000/"; 
        
        /// <summary>
        /// Время жизни токена в минутах
        /// </summary>
        internal const int LIFETIME = 1; 
        
        internal static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}