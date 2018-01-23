using DeliveryTracker.Common;

namespace DeliveryTracker.Identification
{
    /// <summary>
    /// Сущность, хранящая пару Код-Пароль.
    /// </summary>
    public class CodePassword : DictionaryObject
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string Code 
        {
            get => this.Get<string>(nameof(this.Code));
            set => this.Set(nameof(this.Code), value);
        }
        
        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password 
        {
            get => this.Get<string>(nameof(this.Password));
            set => this.Set(nameof(this.Password), value);
        }
    }
}