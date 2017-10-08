
namespace DeliveryTracker.Validation
{
    /// <summary>
    /// Результат проверки параметров.
    /// </summary>
    public class ParametersValidationResult
    {
        public ParametersValidationResult(IError error = null)
        {
            this.Success = error == null;
            this.Error = error;
        }

        /// <summary>
        /// Успешна ли проверка.
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// Ошибка в случае неудачной проверки
        /// В случае удачной проверки null
        /// </summary>
        public IError Error { get; }
    }
}