namespace backend.ViewModels.Errors
{
    public class ErrorItemViewModel
    {
        /// <summary>
        /// Код сообщения об ошибке.
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Текст сообщения об ошибке
        /// </summary>
        public string Message { get; set; }
    }
}