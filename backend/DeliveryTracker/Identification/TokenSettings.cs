using System.Text;
using DeliveryTracker.Common;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryTracker.Identification
{
    /// <summary>
    /// Информация для JWT-токенов
    /// </summary>
    public sealed class TokenSettings : ISettings
    {
        /// <summary>
        /// Приватный ключ для токена.
        /// </summary>
        private readonly string key;
        
        public TokenSettings(
            string name,
            string key,
            string issuer,
            string audience,
            int lifetime,
            int clockCkew,
            bool requireHttps)
        {
            this.Name = name;
            this.key = key;
            this.Issuer = issuer;
            this.Audience = audience;
            this.Lifetime = lifetime;
            this.ClockCkew = clockCkew;
            this.RequireHttps = requireHttps;
        }

        /// <inheritdoc />
        public string Name { get; }
        
        /// <summary>
        /// Издатель токена.
        /// </summary>
        public string Issuer { get; }

        /// <summary>
        /// Потребитель токена.
        /// </summary>
        public string Audience { get; }
        
        /// <summary>
        /// Время жизни токена в минутах.
        /// </summary>
        public int Lifetime { get; }

        /// <summary>
        /// "Перекос" в определении времени жизни.
        /// </summary>
        public int ClockCkew { get; }
        
        /// <summary>
        /// Принимать токены только по https.
        /// </summary>
        public bool RequireHttps { get; }
        
        /// <summary>
        /// Ключ.
        /// </summary>
        /// <returns></returns>
        public SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.key));

    }
}