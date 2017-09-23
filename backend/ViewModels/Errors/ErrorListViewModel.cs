using System.Collections.Generic;

namespace backend.ViewModels.Errors
{
    public class ErrorListViewModel
    {
        /// <summary>
        /// Список ошибок.
        /// </summary>
        public ICollection<ErrorItemViewModel> Errors { get; set; }
    }
}