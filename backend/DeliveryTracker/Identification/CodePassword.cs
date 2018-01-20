namespace DeliveryTracker.Identification
{
    /// <summary>
    /// Сущность, хранящая пару Код-Пароль.
    /// </summary>
    public class CodePassword
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; }
    }
}