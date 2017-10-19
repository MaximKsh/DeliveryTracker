namespace DeliveryTracker.ViewModels
{
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Текущий пароль.
        /// </summary>
        public CredentialsViewModel CurrentCredentials { get; set; }
        
        /// <summary>
        /// Новый пароль.
        /// </summary>
        public CredentialsViewModel NewCredentials { get; set; }
    }
}